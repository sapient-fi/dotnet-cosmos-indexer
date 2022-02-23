using System.Data;
using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using NewRelic.Api.Agent;
using Pylonboard.Kernel.Contracts.Terra;
using Pylonboard.Kernel.DAL.Entities.Terra;
using Pylonboard.Kernel.IdGeneration;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TerraDotnet;
using TerraDotnet.TerraFcd.Messages;
using TerraDotnet.TerraFcd.Messages.Wasm;
using TxLogExtensions = TerraDotnet.Extensions.TxLogExtensions;

namespace Pylonboard.Infrastructure.Hosting.TerraDataFetchers;

public class MineStakingDataFetcher
{
    private readonly ILogger<MineStakingDataFetcher> _logger;
    private readonly TerraTransactionEnumerator _transactionEnumerator;
    private readonly IDbConnectionFactory _dbFactory;
    private readonly IBus _bus;

    public MineStakingDataFetcher(
        ILogger<MineStakingDataFetcher> logger,
        TerraTransactionEnumerator transactionEnumerator,
        IDbConnectionFactory dbFactory,
        IBus bus
    )
    {
        _logger = logger;
        _transactionEnumerator = transactionEnumerator;
        _dbFactory = dbFactory;
        _bus = bus;
    }

    [Trace]
    public async Task FetchDataAsync(CancellationToken stoppingToken, bool fullResync = false)

    {
        _logger.LogInformation("Fetching fresh MINE staking transactions");
        using var db = _dbFactory.OpenDbConnection();

        var latestRow = await db.SingleAsync(
            db.From<TerraMineStakingEntity>()
                .OrderByDescending(q => q.CreatedAt), token: stoppingToken);

        await foreach (var (tx, msg) in _transactionEnumerator.EnumerateTransactionsAsync(
                           0,
                           100,
                           TerraStakingContracts.MINE_STAKING_CONTRACT,
                           stoppingToken))
        {
            // for delta-syncs we check whether the row already exists and then back off
            // for full syncs the consumer has logic to dedupe existing transactions so we wish to pump them all though
            if (!fullResync && tx.Id == latestRow?.TransactionId)
            {
                _logger.LogInformation(
                    "Transaction with id {TxId} and hash {TxHash} already exists, aborting",
                    tx.Id,
                    tx.TransactionHash
                );
                return;
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

            await _bus.Publish(new MineStakingTransactionMessage
                {
                    TransactionId = tx.Id
                },
                stoppingToken
            );
        }

        _logger.LogInformation("Done inserting transactions");
    }
}