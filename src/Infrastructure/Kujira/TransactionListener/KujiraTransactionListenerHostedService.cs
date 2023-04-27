using MassTransit;
using Microsoft.Extensions.Logging;
using SapientFi.Infrastructure.Cosmos;
using SapientFi.Infrastructure.Cosmos.TransactionListener;
using SapientFi.Infrastructure.Kujira.BusMessages;
using SapientFi.Infrastructure.Kujira.Storage;
using TerraDotnet;

namespace SapientFi.Infrastructure.Kujira.TransactionListener;

public class KujiraTransactionListenerHostedService
    : CosmosTransactionListenerHostedService<
        KujiraMarker,
        KujiraRawTransactionEntity,
        RawKujiraTransactionAvailableAnnouncement>
{
    public KujiraTransactionListenerHostedService(
        ILogger<KujiraTransactionListenerHostedService> logger,
        KujiraTransactionEnumerator transactionEnumerator,
        KujiraRawRepository rawRepository,
        CosmosFactory<KujiraRawTransactionEntity> factory,
        IBus massTransitBus
    ) : base(logger, transactionEnumerator, rawRepository, factory, massTransitBus)
    {
    }

    protected override string NameOfBlockChain => "Kujira";
}
