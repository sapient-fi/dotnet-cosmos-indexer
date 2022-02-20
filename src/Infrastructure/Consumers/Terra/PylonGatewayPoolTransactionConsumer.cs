using System.Data;
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

public class PylonGatewayPoolTransactionConsumer : IConsumer<PylonGatewayPoolTransactionMessage>
{
    private readonly ILogger<PylonGatewayPoolTransactionConsumer> _logger;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IdGenerator _idGenerator;

    public PylonGatewayPoolTransactionConsumer(
        ILogger<PylonGatewayPoolTransactionConsumer> logger,
        IDbConnectionFactory dbConnectionFactory,
        IdGenerator idGenerator
    )
    {
        _logger = logger;
        _dbConnectionFactory = dbConnectionFactory;
        _idGenerator = idGenerator;
    }

    public async Task Consume(ConsumeContext<PylonGatewayPoolTransactionMessage> context)
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync();
        using var dbTx = db.OpenTransaction();
        
        var terraTxId = context.Message.TransactionId;
        
        var exists = await db.SingleAsync(db.From<TerraPylonPoolEntity>()
            .Where(q => q.TransactionId == terraTxId 
                        && q.FriendlyName == context.Message.FriendlyName)
            .Take(1), token: context.CancellationToken);

        if (exists != default)
        {
            _logger.LogInformation("Transaction w id {TxId} has already been processed", terraTxId);
            return;
        }
        
        var terraDbTx = await db.SingleByIdAsync<TerraRawTransactionEntity>(terraTxId);
        
        var results = ParseTransaction(
            context.Message.FriendlyName,
            context.Message.PoolContractAddr,
            terraDbTx.RawTx.ToObject<TerraTxWrapper>()!
        );

        if (!results.Any())
        {
            dbTx.Rollback();
            return;
        }
        
        foreach (var entity in results)
        {
            await db.InsertAsync(entity, token: context.CancellationToken);
        }

        dbTx.Commit();
    }

    public List<TerraPylonPoolEntity> ParseTransaction(
        TerraPylonPoolFriendlyName poolFriendlyName,
        string poolContractAddr,
        TerraTxWrapper terraTx
        
        )
    {
        var stdTx = TerraTransactionValueFactory.GetIt(terraTx);
        var results = new List<TerraPylonPoolEntity>();

        foreach (var msg in stdTx.Messages)
        {
            if (msg is not WasmMsgExecuteContract contract)
            {
                _logger.LogDebug("Not a execute contract, skipping for now");
                continue;
            }

            // Deposit, handle!
            
            if (contract.Value.ExecuteMessage?.Deposit != null)
            {
                _logger.LogDebug("Handling deposit");
                // TODO determine deposit denominator?
                var amountDepositStr =
                    terraTx.Logs.QueryTxLogsForAttributeFirstOrDefault("from_contract", "deposit_amount");
                var amount = new TerraAmount(amountDepositStr.Value, TerraDenominators.Ust);

                var data = new TerraPylonPoolEntity
                {
                    Id = _idGenerator.Snowflake(),
                    TransactionId = terraTx.Id,
                    Amount = amount.Value,
                    Denominator = amount.Denominator,
                    Depositor = contract.Value.Sender,
                    Operation = TerraPylonPoolOperation.Deposit,
                    CreatedAt = terraTx.CreatedAt,
                    FriendlyName = poolFriendlyName,
                    PoolContract = poolContractAddr,
                };
                results.Add(data);
                continue;
            }

            if (contract.Value.ExecuteMessage?.Withdraw != null)
            {
                _logger.LogDebug("Handing withdraw");
                var amount = new TerraAmount(contract.Value.ExecuteMessage.Withdraw.Amount, TerraDenominators.Ust);
                var data = new TerraPylonPoolEntity
                {
                    Id = _idGenerator.Snowflake(),
                    TransactionId = terraTx.Id,
                    Amount = -amount.Value,
                    Denominator = amount.Denominator,
                    Depositor = contract.Value.Sender,
                    Operation = TerraPylonPoolOperation.Withdraw,
                    CreatedAt = terraTx.CreatedAt,
                    FriendlyName = poolFriendlyName,
                    PoolContract = poolContractAddr,
                };
                results.Add(data);
                continue;
            }

            if (contract.Value.ExecuteMessage?.Send != null)
            {
                // Send is part of deposit, just skip
                _logger.LogDebug("skipping send msg, already handled in deposit");
                continue;
            }

            if (contract.Value.ExecuteMessage.Claim != null)
            {
                _logger.LogDebug("Handling claim of tokens from pool");
                var claimAmountStr =
                    terraTx.Logs.QueryTxLogsForAttributeFirstOrDefault("from_contract", "claim_amount");
                var denominatorAddrStr = terraTx.Logs
                    .QueryTxLogsForAttributeFirstOrDefault(
                        "from_contract",
                        attribute => attribute.Key.EqualsIgnoreCase("contract_address") &&
                                     !attribute.Value.EqualsIgnoreCase(poolContractAddr));

                var amount = new TerraAmount(
                    claimAmountStr.Value,
                    TerraDenominators.AssetTokenAddressToDenominator[denominatorAddrStr.Value]
                );

                var data = new TerraPylonPoolEntity
                {
                    Id = _idGenerator.Snowflake(),
                    TransactionId = terraTx.Id,
                    Amount = amount.Value,
                    Denominator = amount.Denominator,
                    Depositor = contract.Value.Sender,
                    Operation = TerraPylonPoolOperation.Claim,
                    CreatedAt = terraTx.CreatedAt,
                    FriendlyName = poolFriendlyName,
                    PoolContract = poolContractAddr,
                };
                results.Add(data);
                continue;
            }

            _logger.LogError("Pylon Pool fetcher: unknown situation for tx: {TxHash}.... ", terraTx.TransactionHash);
        }

        return results;
    }
}