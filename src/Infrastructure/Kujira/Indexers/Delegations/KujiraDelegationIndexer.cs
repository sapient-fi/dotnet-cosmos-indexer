using Microsoft.Extensions.Logging;
using SapientFi.Infrastructure.Cosmos.Indexers.Delegations;
using SapientFi.Infrastructure.Indexing;
using SapientFi.Infrastructure.Kujira.BusMessages;
using SapientFi.Infrastructure.Kujira.Indexers.Delegations.Storage;
using SapientFi.Infrastructure.Kujira.Storage;
using SapientFi.Kernel.IdGeneration;

namespace SapientFi.Infrastructure.Kujira.Indexers.Delegations;

public class KujiraDelegationIndexer
    : CosmosDelegationIndexer<
            RawKujiraTransactionAvailableAnnouncement,
            KujiraValidatorDelegationLedgerEntity,
            KujiraRawTransactionEntity>,
        IIndexer<RawKujiraTransactionAvailableAnnouncement>
{
    public KujiraDelegationIndexer(
        ILogger<KujiraDelegationIndexer> logger,
        KujiraDelegationsRepository repository,
        KujiraRawRepository rawRepository,
        IdProvider idProvider
    ) : base(logger, repository, rawRepository, idProvider)
    {
    }

    public override string NameOfBlockChain => "Kujira";
    public override string Id => "kujira_delegations";
    public override string DisplayName => "Kujira Delegations";
}
