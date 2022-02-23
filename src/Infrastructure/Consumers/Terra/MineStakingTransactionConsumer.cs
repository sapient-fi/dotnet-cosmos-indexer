using MassTransit;
using Microsoft.Extensions.Logging;
using Pylonboard.Kernel.Contracts.Terra;
using Pylonboard.Kernel.DAL.Entities.Terra;
using Pylonboard.Kernel.IdGeneration;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TerraDotnet;
using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd;
using TerraDotnet.TerraFcd.Messages;
using TerraDotnet.TerraFcd.Messages.Wasm;

namespace Pylonboard.Infrastructure.Consumers.Terra;

public class MineStakingTransactionConsumer : IConsumer<MineStakingTransactionMessage>
{
    private readonly ILogger<MineStakingTransactionConsumer> _logger;
    private readonly IDbConnectionFactory _dbFactory;
    private readonly IdGenerator _idGenerator;

    public MineStakingTransactionConsumer(
        ILogger<MineStakingTransactionConsumer> logger,
        IDbConnectionFactory dbFactory,
        IdGenerator idGenerator
    )
    {
        _logger = logger;
        _dbFactory = dbFactory;
        _idGenerator = idGenerator;
    }

    public async Task Consume(ConsumeContext<MineStakingTransactionMessage> context)
    {
        using var db = _dbFactory.OpenDbConnection();
        using var tx = db.OpenTransaction();
        var terraTxId = context.Message.TransactionId;
        _logger.LogInformation("Processing MINE gov transaction with id: {TxId}", terraTxId);
        
        var terraDbTx = await db.SingleByIdAsync<TerraRawTransactionEntity>(terraTxId);

        var exists = await db.SingleAsync<long?>(db.From<TerraMineStakingEntity>()
            .Where(q => q.TransactionId == terraTxId)
            .Take(1)
            .Select(e =>
                new
                {
                    e.Id
                }));
        if (exists.HasValue)
        {
            _logger.LogDebug("Mine staking data for transaction id {TxId} already exists", exists);
            tx.Rollback();
            return;
        }

        var terraTx = terraDbTx.RawTx.ToObject<TerraTxWrapper>();
        var msg = TerraTransactionValueFactory.GetIt(terraTx!);
        var cancellationToken = context.CancellationToken;

        var datum = ProcessTransaction(terraTx!, msg);
        foreach (var data in datum)
        {
            await db.InsertAsync(data, token: cancellationToken);
        }

        tx.Commit();
    }

    /// <summary>
    /// Process a single Terra transaction from the blockchain
    /// </summary>
    /// <param name="tx"></param>
    /// <param name="msg"></param>
    /// <returns>A boolean as to whether transaction enumeration should be STOPPED</returns>
    public List<TerraMineStakingEntity> ProcessTransaction(
        TerraTxWrapper tx,
        CoreStdTx msg
    )
    {
        var results = new List<TerraMineStakingEntity>();
        foreach (var properMsg in msg.Messages.Select(innerMsg => innerMsg as WasmMsgExecuteContract))
        {
            if (properMsg == default)
            {
                _logger.LogWarning(
                    "Transaction with id {Id} did not have a message of type {Type}",
                    tx.Id,
                    typeof(WasmMsgExecuteContract)
                );
                continue;
            }

            var amount = 0m;
            if (properMsg.Value.ExecuteMessage?.CastVote != null || properMsg.Value.ExecuteMessage?.EndPoll != null)
            {
                _logger.LogDebug("Cast vote or end poll, ignoring");
                continue;
            }

            if (properMsg.Value.ExecuteMessage?.Mint != null)
            {
                _logger.LogDebug("SPEC mint, ignoring");
                continue;
            }

            if (properMsg.Value.ExecuteMessage?.Sweep != null || properMsg.Value.ExecuteMessage?.Earn != null)
            {
                _logger.LogDebug("Pylon buy-back sweep, handled later");
                continue;
            }

            if (properMsg.Value.ExecuteMessage?.UpdateConfig != null)
            {
                _logger.LogDebug("Contract config update, ignore");
                continue;
            }

            if (properMsg.Value.ExecuteMessage?.Transfer != null)
            {
                _logger.LogDebug("Contract COL-4 transfer, ignore");
                continue;
            }

            if (properMsg.Value.ExecuteMessage?.Send != null)
            {
                if (!properMsg.Value.ExecuteMessage.Send.Contract.EqualsIgnoreCase(
                        TerraStakingContracts.MINE_STAKING_CONTRACT))
                {
                    _logger.LogDebug("Send to contract that is not MINE governance, skipping ");
                    continue;
                }


                amount = Convert.ToDecimal(properMsg.Value.ExecuteMessage.Send.Amount) / 1_000_000m;
            }
            // SPEC farm does funky stuff
            else if (properMsg.Value.ExecuteMessage?.Withdraw != null)
            {
                _logger.LogDebug("Spec farm withdraw, skip");
                continue;
            }
            // SPEC compounding provides tokens for the MINE governance contract
            else if (properMsg.Value.ExecuteMessage.Compound != null)
            {
                _logger.LogDebug("Spec farm compound, skip");
                continue;
            }
            else if (properMsg.Value.ExecuteMessage.Airdrop?.Claim != null)
            {
                _logger.LogDebug("Airdrop claim, skipping");
                continue;
            }
            else if (properMsg.Value.ExecuteMessage.WithdrawVotingTokens != null)
            {
                amount = -1 *
                         (Convert.ToDecimal(properMsg.Value.ExecuteMessage.WithdrawVotingTokens
                             .Amount) / 1_000_000m);
            }
            else if (properMsg.Value.ExecuteMessage.Staking?.Unstake != null)
            {
                amount = -1 * properMsg.Value.ExecuteMessage.Staking.Unstake.Amount.ToInt64() / 1_000_000m;
            }

            else
            {
                _logger.LogWarning(
                    "Transaction w. id {Id} and hash {TxHash} does not have send nor withdraw amount",
                    tx.Id,
                    tx.TransactionHash
                );
                continue;
            }

            var data = new TerraMineStakingEntity
            {
                Id = _idGenerator.Snowflake(),
                TransactionId = tx.Id,
                CreatedAt = tx.CreatedAt.UtcDateTime,
                Sender = properMsg.Value.Sender,
                Amount = amount,
                TxHash = tx.TransactionHash,
                IsBuyBack = false,
            };
            results.Add(data);
        }

        return results;
    }
}