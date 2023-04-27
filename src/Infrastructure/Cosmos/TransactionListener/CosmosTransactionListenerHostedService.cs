using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using SapientFi.Infrastructure.Cosmos.BusMessages;
using SapientFi.Infrastructure.Cosmos.Storage;
using TerraDotnet;
using TerraDotnet.TerraLcd.Messages;
// ReSharper disable MemberCanBePrivate.Global

namespace SapientFi.Infrastructure.Cosmos.TransactionListener;

public abstract class
    CosmosTransactionListenerHostedService<TMarker, TRawTransactionEntity, TRawTransactionAvailableAnnouncement>
    : IHostedService
    where TMarker : CosmosMarker
    where TRawTransactionEntity : ICosmosRawTransactionEntity, new()
    where TRawTransactionAvailableAnnouncement : IRawCosmosTransactionAvailableAnnouncement, new()
{
    protected readonly
        ILogger<CosmosTransactionListenerHostedService<
            TMarker,
            TRawTransactionEntity,
            TRawTransactionAvailableAnnouncement>> Logger;
    protected readonly CosmosTransactionEnumerator<TMarker> TransactionEnumerator;
    protected readonly CosmosRawRepository<TRawTransactionEntity> RawRepository;
    protected readonly CosmosFactory<TRawTransactionEntity> Factory;
    protected readonly IBus MassTransitBus;
    
    protected CancellationTokenSource? TokenSource;
    protected abstract string NameOfBlockChain { get;  }


    protected CosmosTransactionListenerHostedService(
        ILogger<CosmosTransactionListenerHostedService<
            TMarker,
            TRawTransactionEntity,
            TRawTransactionAvailableAnnouncement>> logger,
        CosmosTransactionEnumerator<TMarker> transactionEnumerator,
        CosmosRawRepository<TRawTransactionEntity> rawRepository,
        CosmosFactory<TRawTransactionEntity> factory,
        IBus massTransitBus
    )
    {
        Logger = logger;
        TransactionEnumerator = transactionEnumerator;
        RawRepository = rawRepository;
        Factory = factory;
        MassTransitBus = massTransitBus;
    }

    public virtual async Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Starting {BlockChain} transaction listener", NameOfBlockChain);
        TokenSource = new CancellationTokenSource();

        int latestSeenBlock = await RawRepository.GetLatestSeenBlockHeightAsync(cancellationToken);
        IAsyncEnumerable<LcdTxResponse> enumeration =
            TransactionEnumerator.EnumerateTransactionsAsync(latestSeenBlock, cancellationToken);

        var thread = new Thread(async () =>
            {
                await foreach (LcdTxResponse lcdTransaction in enumeration.WithCancellation(cancellationToken))
                {
                    Logger.LogDebug("Got new raw transaction {TxHash} with height={TxHeight}",
                        lcdTransaction.TransactionHash,
                        lcdTransaction.HeightAsInt
                    );
                    TRawTransactionEntity entity = Factory.NewRawEntity(lcdTransaction);

                    try
                    {
                        await RawRepository.SaveRawTransactionAsync(entity, cancellationToken);

                        await MassTransitBus.Publish(
                            new TRawTransactionAvailableAnnouncement
                            {
                                TransactionHash = entity.TxHash,
                                RawEntityId = entity.Id,
                                CreatedAt = entity.CreatedAt,
                            }, cancellationToken);
                    }
                    catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
                    {
                        // we already have that transaction, so do nothing else
                    }
                }
            }
        );

        Logger.LogInformation("Starting worker thread for {BlockChain}'s transaction listener", NameOfBlockChain);
        thread.Start();
    }

    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Stopping {BlockChain} transactions listener", NameOfBlockChain);
        TokenSource?.Cancel();
        return Task.CompletedTask;
    }
}
