using System.Text.Json;
using Hangfire;
using MassTransit;
using Medallion.Threading;
using NewRelic.Api.Agent;
using Sapient.Kernel.Config;
using Sapient.Kernel.Contracts.Terra;
using Sapient.Kernel.DAL.Entities.Terra;
using Sapient.Kernel.IdGeneration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TerraDotnet;

namespace Sapient.ServiceHost.RecurringJobs;

public class TerraBpsiDpLiquidityPoolTradesRefreshJob
{
    private readonly ILogger<TerraBpsiDpLiquidityPoolTradesRefreshJob> _logger;
    private readonly IEnabledServiceRolesConfig _serviceRolesConfig;
    private readonly TerraTransactionEnumerator _transactionEnumerator;
    private readonly IdGenerator _idGenerator;
    private readonly IDbConnectionFactory _dbFactory;
    private readonly IBus _bus;
    private readonly IDistributedLockProvider _lockProvider;

    public TerraBpsiDpLiquidityPoolTradesRefreshJob(
        ILogger<TerraBpsiDpLiquidityPoolTradesRefreshJob> logger,
        IEnabledServiceRolesConfig serviceRolesConfig,
        TerraTransactionEnumerator transactionEnumerator,
        IdGenerator idGenerator,
        IDbConnectionFactory dbFactory,
        IBus bus,
        IDistributedLockProvider lockProvider
    )
    {
        _logger = logger;
        _serviceRolesConfig = serviceRolesConfig;
        _transactionEnumerator = transactionEnumerator;
        _idGenerator = idGenerator;
        _dbFactory = dbFactory;
        _bus = bus;
        _lockProvider = lockProvider;
    }

    [Trace]
    [AutomaticRetry(Attempts = 0)]
    public async Task DoWorkAsync(
        CancellationToken stoppingToken
    )
    {
        if (!_serviceRolesConfig.IsRoleEnabled(ServiceRoles.BACKGROUND_WORKER))
        {
            _logger.LogInformation("Service role {Role} is not enabled, will not start it",
                ServiceRoles.BACKGROUND_WORKER);
            return;
        }

        await using var theLock = await _lockProvider.TryAcquireLockAsync("locks:job:bpsi-liquid", TimeSpan.Zero,
            cancellationToken: stoppingToken);
        if (theLock == default)
        {
            // the lock is a null instance meaning that we FAILED to acquire it... Abort basically
            _logger.LogWarning("Another bpsi dp refresh job is holding the lock, aborting");
            return;
        }
        
        _logger.LogInformation("Fetching fresh bPSI DP liquidity pool data");
        using var db = _dbFactory.OpenDbConnection();

        var latestRow = await db.SingleAsync(
            db.From<TerraLiquidityPoolTradesEntity>()
                .OrderByDescending(q => q.CreatedAt), token: stoppingToken);

        await foreach (var (terraTx, msg) in _transactionEnumerator.EnumerateTransactionsAsync(
                           0,
                           100,
                           TerraLiquidityPoolContracts.BPSI_DP_CONTRACT,
                           stoppingToken))
        {
            if (terraTx.Id == latestRow?.TransactionId)
            {
                _logger.LogInformation("Transaction with id {TxId} and hash {TxHash} already exists, no futher bus publishes", terraTx.Id,
                    terraTx.TransactionHash);
                break;
            }
            await db.SaveAsync(obj: new TerraRawTransactionEntity
                {
                    Id = terraTx.Id,
                    CreatedAt = terraTx.CreatedAt,
                    TxHash = terraTx.TransactionHash,
                    RawTx = JsonSerializer.Serialize(terraTx),
                },
                token: stoppingToken
            );

            await _bus.Publish(new BPsiTerraTransactionMessage
            {
                TransactionId = terraTx.Id,
            }, stoppingToken);
        }
    }
}