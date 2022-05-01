using System.Data;
using Pylonboard.Infrastructure.Oracles.ExchangeRates.Terra;
using Pylonboard.Kernel;
using Pylonboard.Kernel.Config;
using Pylonboard.Kernel.DAL.Entities.Exchanges;
using Pylonboard.Kernel.IdGeneration;
using Pylonboard.ServiceHost.Config;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TerraDotnet;

namespace Pylonboard.ServiceHost.RecurringJobs;

public class FxRateDownloadJob
{
    private readonly ILogger<FxRateDownloadJob> _logger;
    private readonly IEnabledServiceRolesConfig _serviceRolesConfig;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly TerraExchangeRateOracle _terraExchangeRateOracle;
    private readonly IdGenerator _idGenerator;

    public FxRateDownloadJob(
        ILogger<FxRateDownloadJob> logger,
        IEnabledServiceRolesConfig serviceRolesConfig,
        IDbConnectionFactory dbConnectionFactory,
        TerraExchangeRateOracle terraExchangeRateOracle,
        IdGenerator idGenerator
    )
    {
        _logger = logger;
        _serviceRolesConfig = serviceRolesConfig;
        _dbConnectionFactory = dbConnectionFactory;
        _terraExchangeRateOracle = terraExchangeRateOracle;
        _idGenerator = idGenerator;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        if (!_serviceRolesConfig.IsRoleEnabled(ServiceRoles.BACKGROUND_WORKER))
        {
            _logger.LogInformation("Background worker role not active, not starting materialized view refrsher");
            return;
        }


        _logger.LogInformation("FX rate downloader is active - downloading rates");

        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: stoppingToken);
        {
            await FetchAndStoreRateAsync(TerraDenominators.Anc, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Apollo, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Glow, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Loop, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Loopr, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Luna, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.nLuna, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Mine, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Mir, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.nEth, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.bEth, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Orion, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Psi, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Sayve, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Stt, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Twd, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Vkr, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Whale, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Xdefi, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Arts, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.WCoin, TerraDenominators.Ust, db, stoppingToken);
            await FetchAndStoreRateAsync(TerraDenominators.Sayve, TerraDenominators.Ust, db, stoppingToken);
        }
        _logger.LogInformation("FX rate downloader completed");
    }

    private async Task FetchAndStoreRateAsync(string from, string to, IDbConnection db, CancellationToken stoppingToken)
    {
        var (close, closeAt) =
            await _terraExchangeRateOracle.GetExchangeRateAsync(from, to, DateTimeOffset.UtcNow, interval: "15m");
        await db.InsertAsync<ExchangeMarketCandle>(new ExchangeMarketCandle
        {
            CloseTime = closeAt,
            OpenTime = closeAt.AddMinutes(-15),
            Close = close,
            Exchange = Exchange.Terra,
            Market = $"{from.ToUpperInvariant()}-{to.ToUpperInvariant()}",
            Id = _idGenerator.Snowflake(),
        }, token: stoppingToken);
    }
}