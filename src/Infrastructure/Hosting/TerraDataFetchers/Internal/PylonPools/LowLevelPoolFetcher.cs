using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using NewRelic.Api.Agent;
using Sapient.Kernel.Contracts.Terra;
using Sapient.Kernel.DAL.Entities.Terra;
using Sapient.Kernel.IdGeneration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TerraDotnet;

namespace Sapient.Infrastructure.Hosting.TerraDataFetchers.Internal.PylonPools;

public class LowLevelPoolFetcher
{
    private readonly TerraTransactionEnumerator _transactionEnumerator;
    private readonly IDbConnectionFactory _dbFactory;
    private readonly ILogger<LowLevelPoolFetcher> _logger;
    private readonly IBus _bus;

    public LowLevelPoolFetcher(
        TerraTransactionEnumerator transactionEnumerator,
        IDbConnectionFactory dbFactory,
        IdGenerator idGenerator,
        ILogger<LowLevelPoolFetcher> logger,
        IBus bus
    )
    {
        _transactionEnumerator = transactionEnumerator;
        _dbFactory = dbFactory;
        _logger = logger;
        _bus = bus;
    }

    [Trace]
    public async Task FetchPoolDataAsync(
        string pylonPoolContractAddr,
        TerraPylonPoolFriendlyName friendlyName,
        CancellationToken stoppingToken,
        bool fullResync
    )
    {
        _logger.LogInformation("Fetching GW pool data for {Pool} in full resync mode? {FullResync}", friendlyName,
            fullResync);
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
            if (tx.Id == latestRow?.TransactionId && !fullResync)
            {
                _logger.LogInformation("Transaction with id {TxId} and hash {TxHash} already exists, aborting", tx.Id,
                    tx.TransactionHash);
                break;
            }

            await db.SaveAsync(obj: new TerraRawTransactionEntity
                {
                    Id = tx.Id,
                    CreatedAt = tx.CreatedAt,
                    TxHash = tx.TransactionHash,
                    RawTx = JsonSerializer.Serialize(tx),
                },
                token: stoppingToken
            );

            await _bus.Publish(new PylonGatewayPoolTransactionMessage
            {
                FriendlyName = friendlyName,
                TransactionId = tx.Id,
                PoolContractAddr = pylonPoolContractAddr,
            }, stoppingToken);
        }
    }
}