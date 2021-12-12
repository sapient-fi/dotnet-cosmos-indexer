using System.Text.Json;
using Pylonboard.Kernel.IdGeneration;
using Pylonboard.ServiceHost.DAL.TerraMoney;
using Pylonboard.ServiceHost.Oracles;
using Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TxLogExtensions = Pylonboard.ServiceHost.Extensions.TxLogExtensions;

namespace Pylonboard.ServiceHost.TerraDataFetchers;

public class MineStakingDataFetcher
{
    private readonly ILogger<MineStakingDataFetcher> _logger;
    private readonly TerraTransactionEnumerator _transactionEnumerator;
    private readonly IdGenerator _idGenerator;
    private readonly IDbConnectionFactory _dbFactory;

    public MineStakingDataFetcher(
        ILogger<MineStakingDataFetcher> logger,
        TerraTransactionEnumerator transactionEnumerator,
        IdGenerator idGenerator,
        IDbConnectionFactory dbFactory
    )
    {
        _logger = logger;
        _transactionEnumerator = transactionEnumerator;
        _idGenerator = idGenerator;
        _dbFactory = dbFactory;
    }

    public async Task FetchDataAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Fetching fresh MINE staking transactions");
        int skipped = 0;
        using var db = _dbFactory.OpenDbConnection();

        await foreach (var (tx, msg) in _transactionEnumerator.EnumerateTransactionsAsync(
                           0,
                           100,
                           TerraStakingContracts.MINE_STAKING_CONTRACT,
                           stoppingToken))
        {
            if (skipped >= 50)
            {
                _logger.LogInformation("50 transactions have now been skipped, stopping enumeration");
                break;
            }

            using var dbTx = db.BeginTransaction();
            await dbTx.Connection.SaveAsync(obj: new TerraRawTransactionEntity
                {
                    Id = tx.Id,
                    CreatedAt = tx.CreatedAt,
                    TxHash = tx.TransactionHash,
                    RawTx = JsonSerializer.Serialize(tx),
                },
                token: stoppingToken
            );

            var exists =
                await db.SingleAsync<TerraMineStakingEntity>(q => q.TxHash == tx.TransactionHash,
                    token: stoppingToken);
            if (exists != default)
            {
                _logger.LogDebug("tx exists for {Id}", tx.Id);
                skipped++;
                continue;
            }

            // reset skip-counter
            skipped = 0;
            foreach (var properMsg in msg.Messages.Select(innerMsg => innerMsg as WasmMsgExecuteContract))
            {
                if (properMsg == default)
                {
                    _logger.LogWarning("Transaction with id {Id} did not have a message of type {Type}", tx.Id,
                        typeof(WasmMsgExecuteContract));
                    continue;
                }

                var amount = 0m;
                if (properMsg.Value.ExecuteMessage.CastVote != null || properMsg.Value.ExecuteMessage.EndPoll != null)
                {
                    _logger.LogDebug("Cast vote or end poll, ignoring");
                    continue;
                }

                if (properMsg.Value.ExecuteMessage.Mint != null)
                {
                    _logger.LogDebug("SPEC mint, ignoring");
                    continue;
                }

                if (properMsg.Value.ExecuteMessage.Sweep != null || properMsg.Value.ExecuteMessage.Earn != null)
                {
                    _logger.LogDebug("Pylon buy-back sweep, handled later");
                    continue;
                }

                if (properMsg.Value.ExecuteMessage.UpdateConfig != null)
                {
                    _logger.LogDebug("Contract config update, ignore");
                    continue;
                }

                if (properMsg.Value.ExecuteMessage.Transfer != null)
                {
                    _logger.LogDebug("Contact COL-4 transfer, ignore");
                    continue;
                }
                    
                if (properMsg.Value.ExecuteMessage.Send != null)
                {
                    amount = Convert.ToDecimal(properMsg.Value.ExecuteMessage.Send.Amount) / 1_000_000m;
                }
                // SPEC farm does funky stuff
                else if (properMsg.Value.ExecuteMessage.Withdraw != null)
                {
                    _logger.LogDebug("Spec farm withdraw, skip");
                    continue;
                    var contractAddresses = TxLogExtensions.QueryTxLogsForAttributes(tx.Logs, "from_contract",
                            attribute => StringExtensions.EqualsIgnoreCase(attribute.Key, "contract_address"))
                        .ToList();

                    if (contractAddresses.Any(attribute =>
                            attribute.Value.EqualsIgnoreCase(TerraStakingContracts.SPEC_PYLON_FARM)))
                    {
                        if (contractAddresses.Any(attribute =>
                                attribute.Value.EqualsIgnoreCase(TerraStakingContracts.MINE_STAKING_CONTRACT)))
                        {
                            var farmAmount =
                                TxLogExtensions.QueryTxLogsForAttributeFirstOrDefault(tx.Logs, "from_contract", "farm_amount");
                            amount = farmAmount.Value.ToInt64() / 1_000_000m;
                        }
                    }
                        
                }
                // SPEC compounding provides tokens for the MINE governance contract
                else if (properMsg.Value.ExecuteMessage.Compound != null)
                {
                    _logger.LogDebug("Spec farm compound, skip");
                    continue;
                    var contractAddresses = TxLogExtensions.QueryTxLogsForAttributes(tx.Logs, "from_contract",
                            attribute => StringExtensions.EqualsIgnoreCase(attribute.Key, "contract_address"))
                        .ToList();

                    if (contractAddresses.Any(attribute =>
                            attribute.Value.EqualsIgnoreCase(TerraStakingContracts.SPEC_PYLON_FARM)))
                    {
                        if (contractAddresses.Any(attribute =>
                                attribute.Value.EqualsIgnoreCase(TerraStakingContracts.MINE_STAKING_CONTRACT)))
                        {
                            var stakeAmount =
                                TxLogExtensions.QueryTxLogsForAttributeFirstOrDefault(tx.Logs, "from_contract", "stake_amount");
                            amount = stakeAmount.Value.ToInt64() / 1_000_000m;
                        }
                    }

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
                    // throw new NotImplementedException("Cannot handle this TX for some reason");
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
                await dbTx.Connection.SaveAsync(data, token: stoppingToken);
            }

            dbTx.Commit();
        }

        _logger.LogInformation("Done inserting transactions");
    }
}