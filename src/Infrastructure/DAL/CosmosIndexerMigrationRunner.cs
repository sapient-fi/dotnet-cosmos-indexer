using Microsoft.Extensions.Logging;
using RapidCore.DependencyInjection;
using RapidCore.Locking;
using RapidCore.Migration;

namespace SapientFi.Infrastructure.DAL;

public class CosmosIndexerMigrationRunner : MigrationRunner
{
    public CosmosIndexerMigrationRunner(
        ILogger<CosmosIndexerMigrationRunner> logger,
        IRapidContainerAdapter container,
        IMigrationEnvironment environment,
        IDistributedAppLockProvider appLocker,
        IMigrationContextFactory contextFactory,
        IMigrationFinder finder,
        IMigrationStorage storage
    ) : base(logger, container, environment, appLocker, contextFactory, finder, storage)
    {
    }

    protected override string GetLockName()
    {
        return "locks:migrations:cosmos-indexer";
    }
}