using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using SapientFi.Infrastructure.Kujira.BusMessages;
using SapientFi.Infrastructure.Kujira.Storage;
using TerraDotnet;

namespace SapientFi.Infrastructure.Kujira.TransactionListener;

public class KujiraTransactionListenerHostedService : IHostedService
{
    private readonly ILogger<KujiraTransactionListenerHostedService> _logger;
    private readonly CosmosTransactionEnumerator<KujiraMarker> _transactionEnumerator;
    private readonly KujiraRawRepository _rawRepository;
    private readonly KujiraFactory _factory;
    private readonly IBus _massTransitBus;

    private CancellationTokenSource? _tokenSource;

    public KujiraTransactionListenerHostedService(
        ILogger<KujiraTransactionListenerHostedService> logger,
        CosmosTransactionEnumerator<KujiraMarker> transactionEnumerator,
        KujiraRawRepository rawRepository,
        KujiraFactory factory, 
        IBus massTransitBus
    )
    {
        _logger = logger;
        _transactionEnumerator = transactionEnumerator;
        _rawRepository = rawRepository;
        _factory = factory;
        _massTransitBus = massTransitBus;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting kujira transaction listener");
        _tokenSource = new CancellationTokenSource();

        var latestSeenBlock = await _rawRepository.GetLatestSeenBlockHeightAsync(cancellationToken);

        var enumeration = _transactionEnumerator.EnumerateTransactionsAsync(latestSeenBlock, cancellationToken);

        await foreach (var lcdTransaction in enumeration.WithCancellation(cancellationToken))
        {
            _logger.LogDebug("Got new raw transaction {TxHash} with height={TxHeight}", lcdTransaction.TransactionHash, lcdTransaction.HeightAsInt);

            var entity = _factory.NewRawEntity(lcdTransaction);

            try
            {
                await _rawRepository.SaveRawTransactionAsync(entity, cancellationToken);

                await _massTransitBus.Publish(
                    new RawKujiraTransactionAvailableAnnouncement
                    {
                        TransactionHash = entity.TxHash,
                        RawEntityId = entity.Id
                    }, cancellationToken);
            }
            catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                // we already have that transaction, so do nothing else
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping terra2 transaction listener");
        _tokenSource?.Cancel();
        return Task.CompletedTask;
    }
}