using System.Data;
using System.Text.Json;
using Hangfire;
using MassTransit;
using NewRelic.Api.Agent;
using Pylonboard.Kernel;
using Pylonboard.Kernel.IdGeneration;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.Consumers.Terra;
using Pylonboard.ServiceHost.DAL.TerraMoney;
using Pylonboard.ServiceHost.Extensions;
using Pylonboard.ServiceHost.Oracles;
using Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;
using Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;
using Pylonboard.ServiceHost.TerraDataFetchers;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.RecurringJobs;

public class TerraBpsiDpLiquidityPoolRefreshJob
{
    private readonly ILogger<TerraBpsiDpLiquidityPoolRefreshJob> _logger;
    private readonly IEnabledServiceRolesConfig _serviceRolesConfig;
    private readonly TerraTransactionEnumerator _transactionEnumerator;
    private readonly IdGenerator _idGenerator;
    private readonly IDbConnectionFactory _dbFactory;
    private readonly IBus _bus;

    public TerraBpsiDpLiquidityPoolRefreshJob(
        ILogger<TerraBpsiDpLiquidityPoolRefreshJob> logger,
        IEnabledServiceRolesConfig serviceRolesConfig,
        TerraTransactionEnumerator transactionEnumerator,
        IdGenerator idGenerator,
        IDbConnectionFactory dbFactory,
        IBus bus
    )
    {
        _logger = logger;
        _serviceRolesConfig = serviceRolesConfig;
        _transactionEnumerator = transactionEnumerator;
        _idGenerator = idGenerator;
        _dbFactory = dbFactory;
        _bus = bus;
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

        _logger.LogInformation("Fetching fresh bPSI DP liquidity pool data");
        using var db = _dbFactory.OpenDbConnection();

        var latestRow = await db.SingleAsync(
            db.From<TerraLiquidityPoolEntity>()
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