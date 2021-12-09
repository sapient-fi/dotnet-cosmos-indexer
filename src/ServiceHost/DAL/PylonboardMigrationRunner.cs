using RapidCore.DependencyInjection;
using RapidCore.Locking;
using RapidCore.Migration;

namespace Pylonboard.ServiceHost.DAL;

public class PylonboardMigrationRunner : MigrationRunner
{
    public PylonboardMigrationRunner(
        ILogger<PylonboardMigrationRunner> logger,
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
        return "locks:migrations:pylonboard";
    }
}