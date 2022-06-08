using System.Text.Json;
using Hangfire;
using MassTransit;
using NewRelic.Api.Agent;
using Sapient.Kernel;
using Sapient.Kernel.Config;
using Sapient.Kernel.Contracts.Terra;
using Sapient.Kernel.DAL.Entities.Terra;
using Sapient.Kernel.IdGeneration;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TerraDotnet;

namespace Sapient.ServiceHost.RecurringJobs;

public class TerraLiquidityPoolPairRefreshJob
{
    private readonly ILogger<TerraLiquidityPoolPairRefreshJob> _logger;
    private readonly IEnabledServiceRolesConfig _serviceRolesConfig;
    private readonly TerraTransactionEnumerator _transactionEnumerator;
    private readonly IdGenerator _idGenerator;
    private readonly IDbConnectionFactory _dbFactory;
    private readonly IBus _bus;

    public TerraLiquidityPoolPairRefreshJob(
        ILogger<TerraLiquidityPoolPairRefreshJob> logger,
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
        string lpPairTokenAddress,
        DecentralizedExchange dex,
        CancellationToken stoppingToken
    )
    {
        if (!_serviceRolesConfig.IsRoleEnabled(ServiceRoles.BACKGROUND_WORKER))
        {
            _logger.LogInformation("Service role {Role} is not enabled, will not start it",
                ServiceRoles.BACKGROUND_WORKER);
            return;
        }

        _logger.LogInformation("Fetching fresh terra liquidity pool pair data for {PairAddress}", lpPairTokenAddress);
        using var db = _dbFactory.OpenDbConnection();

        var latestRow = await db.SingleAsync(
            db.From<TerraLiquidityPoolPairEntity>()
                .OrderByDescending(q => q.CreatedAt), token: stoppingToken);
        
        await foreach (var (terraTx, msg) in _transactionEnumerator.EnumerateTransactionsAsync(
                           0,
                           100,
                           lpPairTokenAddress,
                           stoppingToken))
        {
            if (terraTx.Id == latestRow?.TransactionId)
            {
                _logger.LogInformation(
                    "Transaction with id {TxId} and hash {TxHash} already exists, no further action", terraTx.Id,
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

            await _bus.Publish(new TerraLiquidityPoolPairTransactionMessage
            {
                TransactionId = terraTx.Id,
                Dex = dex
            }, stoppingToken);
        }
    }
}