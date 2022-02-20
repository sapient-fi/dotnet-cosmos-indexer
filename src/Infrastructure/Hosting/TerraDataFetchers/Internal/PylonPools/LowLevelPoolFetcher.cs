using System.Text.Json;
using Microsoft.Extensions.Logging;
using NewRelic.Api.Agent;
using Pylonboard.Kernel.DAL.Entities.Terra;
using Pylonboard.Kernel.IdGeneration;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TerraDotnet;
using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd.Messages.Wasm;

namespace Pylonboard.Infrastructure.Hosting.TerraDataFetchers.Internal.PylonPools;

public class LowLevelPoolFetcher
{
    private readonly TerraTransactionEnumerator _transactionEnumerator;
    private readonly IDbConnectionFactory _dbFactory;
    private readonly IdGenerator _idGenerator;
    private readonly ILogger<LowLevelPoolFetcher> _logger;

    public LowLevelPoolFetcher(
        TerraTransactionEnumerator transactionEnumerator,
        IDbConnectionFactory dbFactory,
        IdGenerator idGenerator,
        ILogger<LowLevelPoolFetcher> logger
    )
    {
        _transactionEnumerator = transactionEnumerator;
        _dbFactory = dbFactory;
        _idGenerator = idGenerator;
        _logger = logger;
    }

    [Trace]
    public async Task FetchPoolDataAsync(string pylonPoolContractAddr, TerraPylonPoolFriendlyName friendlyName,
        CancellationToken stoppingToken, bool fullResync)
    {
        _logger.LogInformation("Fetching GW pool data for {Pool} in full resync mode? {FullResync}", friendlyName, fullResync);
        var agent = NewRelic.Api.Agent.NewRelic.GetAgent(); 
        var currentSpan = agent.CurrentSpan;
        currentSpan?.AddCustomAttribute("pool-name", friendlyName);
        
        const long offset = 0;
        using var db = _dbFactory.OpenDbConnection();
        var latestRow = await db.SingleAsync(db.From<TerraPylonPoolEntity>()
            .Where(q => q.FriendlyName == friendlyName)
            .OrderByDescending(q => q.CreatedAt)
            .Take(1), token: stoppingToken);
        
        await foreach (var (tx, stdTx) in _transactionEnumerator.EnumerateTransactionsAsync(
                           offset,
                           100,
                           pylonPoolContractAddr,
                           stoppingToken
                       ))
        {
            if (fullResync)
            {
                var exists = await db.SingleAsync(db.From<TerraPylonPoolEntity>()
                    .Where(q => q.TransactionId == tx.Id 
                                && q.FriendlyName == friendlyName)
                    .Take(1), token: stoppingToken);

                if (exists != default)
                {
                    _logger.LogInformation("Transaction with id {TxId} and hash {TxHash} already exists, skipping during full resync", tx.Id, tx.TransactionHash);
                    continue;
                }
            }
            else if (tx.Id == latestRow?.TransactionId)
            {
                _logger.LogInformation("Transaction with id {TxId} and hash {TxHash} already exists, aborting", tx.Id, tx.TransactionHash);
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

            foreach (var msg in stdTx.Messages)
            {
                if(msg is not WasmMsgExecuteContract contract)
                {
                    _logger.LogDebug("Not a execute contract, skipping for now");
                    continue;
                }
                // Deposit, handle!
                if (contract.Value.ExecuteMessage.Deposit != null)
                {
                    _logger.LogDebug("Handling deposit");
                    // TODO determine deposit denominator?
                    var amountDepositStr =
                        tx.Logs.QueryTxLogsForAttributeFirstOrDefault("from_contract", "deposit_amount");
                    var amount = new TerraAmount(amountDepositStr.Value, TerraDenominators.Ust);

                    var data = new TerraPylonPoolEntity
                    {
                        Id = _idGenerator.Snowflake(),
                        TransactionId = tx.Id,
                        Amount = amount.Value,
                        Denominator = amount.Denominator,
                        Depositor = contract.Value.Sender,
                        Operation = TerraPylonPoolOperation.Deposit,
                        CreatedAt = tx.CreatedAt,
                        FriendlyName = friendlyName,
                        PoolContract = pylonPoolContractAddr,
                    };
                    await db.InsertAsync(data, token: stoppingToken);
                    continue;
                }

                if (contract.Value.ExecuteMessage.Withdraw != null)
                {
                    _logger.LogDebug("Handing withdraw");
                    var amount = new TerraAmount(contract.Value.ExecuteMessage.Withdraw.Amount, TerraDenominators.Ust);
                    var data = new TerraPylonPoolEntity
                    {
                        Id = _idGenerator.Snowflake(),
                        TransactionId = tx.Id,
                        Amount = -amount.Value,
                        Denominator = amount.Denominator,
                        Depositor = contract.Value.Sender,
                        Operation = TerraPylonPoolOperation.Withdraw,
                        CreatedAt = tx.CreatedAt,
                        FriendlyName = friendlyName,
                        PoolContract = pylonPoolContractAddr,
                    };
                    await db.InsertAsync(data, token: stoppingToken);
                    continue;
                }

                if (contract.Value.ExecuteMessage.Send != null)
                {
                    // Send is part of deposit, just skip
                    _logger.LogDebug("skipping send msg, already handled in deposit");
                    continue;
                }

                if (contract.Value.ExecuteMessage.Claim != null)
                {
                    _logger.LogDebug("Handling claim of tokens from pool");
                    var claimAmountStr =
                        tx.Logs.QueryTxLogsForAttributeFirstOrDefault("from_contract", "claim_amount");
                    var denominatorAddrStr = tx.Logs
                        .QueryTxLogsForAttributeFirstOrDefault(
                            "from_contract",
                            attribute => attribute.Key.EqualsIgnoreCase("contract_address") && ! attribute.Value.EqualsIgnoreCase(pylonPoolContractAddr));

                    var amount = new TerraAmount(
                        claimAmountStr.Value,
                        TerraDenominators.AssetTokenAddressToDenominator[denominatorAddrStr.Value]
                    );

                    var data = new TerraPylonPoolEntity
                    {
                        Id = _idGenerator.Snowflake(),
                        TransactionId = tx.Id,
                        Amount = amount.Value,
                        Denominator = amount.Denominator,
                        Depositor = contract.Value.Sender,
                        Operation = TerraPylonPoolOperation.Claim,
                        CreatedAt = tx.CreatedAt,
                        FriendlyName = friendlyName,
                        PoolContract = pylonPoolContractAddr,
                    };
                    await db.InsertAsync(data, token: stoppingToken);
                    continue;
                }
                _logger.LogError("Pylon Pool fetcher: unknown situation for tx: {TxHash}.... ", tx.TransactionHash);
            }

            dbTx.Commit();
        }
    }
}