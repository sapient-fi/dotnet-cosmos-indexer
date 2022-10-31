using Microsoft.Extensions.Logging;
using SapientFi.Infrastructure.Cosmos.Indexers.Delegations;
using SapientFi.Infrastructure.Indexing;
using SapientFi.Infrastructure.Terra2.BusMessages;
using SapientFi.Infrastructure.Terra2.Indexers.Delegations.Storage;
using SapientFi.Infrastructure.Terra2.Storage;
using SapientFi.Kernel.IdGeneration;

namespace SapientFi.Infrastructure.Terra2.Indexers.Delegations;

public class Terra2DelegationIndexer
    : CosmosDelegationIndexer<
            RawTerra2TransactionAvailableAnnouncement,
            Terra2ValidatorDelegationLedgerEntity,
            Terra2RawTransactionEntity>,
        IIndexer<RawTerra2TransactionAvailableAnnouncement>
{
    public Terra2DelegationIndexer(
        ILogger<Terra2DelegationIndexer> logger,
        Terra2DelegationsRepository repository,
        Terra2RawRepository rawRepository,
        IdProvider idProvider
    ) : base(logger, repository, rawRepository, idProvider)
    {
    }
    public override string NameOfBlockChain => "Terra2";
    public override string Id => "terra2_delegations";
    public override string DisplayName => "Terra2 Delegations";
}