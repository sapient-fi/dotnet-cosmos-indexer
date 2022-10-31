using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using SapientFi.Infrastructure.Terra2.BusMessages;
using SapientFi.Infrastructure.Terra2.Storage;
using TerraDotnet;

namespace SapientFi.Infrastructure.Terra2.TransactionListener;

public class Terra2TransactionListenerHostedService : IHostedService
{
    private readonly ILogger<Terra2TransactionListenerHostedService> _logger;
    private readonly CosmosTransactionEnumerator<Terra2Marker> _transactionEnumerator;
    private readonly Terra2RawRepository _rawRepository;
    private readonly Terra2Factory _factory;
    private readonly IBus _massTransitBus;

    private CancellationTokenSource? _tokenSource;

    public Terra2TransactionListenerHostedService(
        ILogger<Terra2TransactionListenerHostedService> logger,
        CosmosTransactionEnumerator<Terra2Marker> transactionEnumerator,
        Terra2RawRepository rawRepository,
        Terra2Factory factory, 
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
        // FIXME dead code on purpose
        return;
        
        _logger.LogInformation("Starting terra2 transaction listener");
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
                    new RawTerra2TransactionAvailableAnnouncement
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