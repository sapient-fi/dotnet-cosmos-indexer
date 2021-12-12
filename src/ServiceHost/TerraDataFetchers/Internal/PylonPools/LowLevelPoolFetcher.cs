using System.Text.Json;
using Pylonboard.Kernel;
using Pylonboard.Kernel.IdGeneration;
using Pylonboard.ServiceHost.DAL.TerraMoney;
using Pylonboard.ServiceHost.Extensions;
using Pylonboard.ServiceHost.Oracles;
using Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.TerraDataFetchers.Internal.PylonPools;

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

    public async Task FetchPoolDataAsync(string pylonPoolContractAddr, TerraPylonPoolFriendlyName friendlyName, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Fetching GW pool data for {Pool}", friendlyName);
        
        const long offset = 0;
        int skipped = 0;
        using var db = _dbFactory.OpenDbConnection();
        await foreach (var (tx, stdTx) in _transactionEnumerator.EnumerateTransactionsAsync(
                           offset,
                           100,
                           pylonPoolContractAddr,
                           stoppingToken
                       ))
        {
            if (skipped > 50)
            {
                _logger.LogInformation("50 txes skipped, won't fetch more txes");
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
                await dbTx.Connection.SelectAsync<TerraPylonPoolEntity>(q => q.TransactionId == tx.Id,
                    token: stoppingToken);
            if (exists.Any())
            {
                _logger.LogDebug("Pylon Gateway tx already exists: {TxHash}", tx.TransactionHash);
                skipped++;
                continue;
            }

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
                    var amount = new TerraAmount
                    {
                        Denominator = "UST",
                        Value = amountDepositStr.Value.ToInt64() / 1_000_000m,
                    };

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
                    var amount = new TerraAmount
                    {
                        Denominator = TerraDenominators.Ust,
                        Value = contract.Value.ExecuteMessage.Withdraw.Amount.ToInt64() / 1_000_000M,
                    };
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

                    var amount = new TerraAmount
                    {
                        Value = claimAmountStr.Value.ToInt64() / 1_000_000m,
                        Denominator = TerraDenominators.AssetTokenAddressToDenominator[denominatorAddrStr.Value]
                    };

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