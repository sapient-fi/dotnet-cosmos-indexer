using Polly;
using Pylonboard.Kernel;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.DAL.Exchanges;
using Pylonboard.ServiceHost.Oracles.ExchangeRates.Terra;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.RecurringJobs;

public class PsiPoolArbJob
{
    private readonly ILogger<PsiPoolArbJob> _logger;
    private readonly TerraExchangeRateOracle _exchangeRateOracle;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IEnabledServiceRolesConfig _serviceRolesConfig;

    public PsiPoolArbJob(
        ILogger<PsiPoolArbJob> logger,
        TerraExchangeRateOracle exchangeRateOracle,
        IDbConnectionFactory dbConnectionFactory,
        IEnabledServiceRolesConfig serviceRolesConfig
    )
    {
        _logger = logger;
        _exchangeRateOracle = exchangeRateOracle;
        _dbConnectionFactory = dbConnectionFactory;
        _serviceRolesConfig = serviceRolesConfig;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        if (!_serviceRolesConfig.IsRoleEnabled(ServiceRoles.BACKGROUND_WORKER))
        {
            _logger.LogInformation("Background worker role not active, not starting arb bot");
            return;
        }
        
        var now = DateTimeOffset.Now;
        await Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                5,
                retryCounter => TimeSpan.FromMilliseconds(Math.Pow(10, retryCounter)),
                (exception, span) =>
                {
                    _logger.LogWarning("Handling retry while performing Terra Transactions, waiting {Time:c}", span);
                }
            )
            .ExecuteAsync(async () =>
            {
                var toPsi = await _exchangeRateOracle.GetExchangeRateAsync(TerraDenominators.bPsiDP,
                    TerraDenominators.Psi, now);
                var toUst = await _exchangeRateOracle.GetExchangeRateAsync(TerraDenominators.Psi,
                    TerraDenominators.Ust, now);

                var endResult = toUst.close * toPsi.close;
                using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: stoppingToken);
                {
                    await db.InsertAsync(new ExchangeMarketCandle
                    {
                        Close = endResult,
                        Exchange = Exchange.Terra,
                        Market = $"{TerraDenominators.bPsiDP}-arb",
                        CloseTime = toUst.closedAt,
                    }, token: stoppingToken);
                }
            });
    }
}