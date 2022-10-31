using MassTransit;
using Microsoft.Extensions.Logging;
using SapientFi.Infrastructure.Cosmos;
using SapientFi.Infrastructure.Cosmos.TransactionListener;
using SapientFi.Infrastructure.Terra2.BusMessages;
using SapientFi.Infrastructure.Terra2.Storage;
using TerraDotnet;

namespace SapientFi.Infrastructure.Terra2.TransactionListener;

public class Terra2TransactionListenerHostedService
    : CosmosTransactionListenerHostedService<
        Terra2Marker,
        Terra2RawTransactionEntity,
        RawTerra2TransactionAvailableAnnouncement>
{
    public Terra2TransactionListenerHostedService(
        ILogger<Terra2TransactionListenerHostedService> logger,
        Terra2TransactionEnumerator transactionEnumerator,
        Terra2RawRepository rawRepository,
        CosmosFactory<Terra2RawTransactionEntity> factory,
        IBus massTransitBus
    ) : base(logger, transactionEnumerator, rawRepository, factory, massTransitBus)
    {
    }

    protected override string NameOfBlockChain => "Terra2";
}