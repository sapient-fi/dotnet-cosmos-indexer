using Polly;
using Pylonboard.Kernel;
using Pylonboard.Kernel.Hosting.BackgroundWorkers;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.DAL.Exchanges;
using Pylonboard.ServiceHost.Oracles.ArbNotifier;
using Pylonboard.ServiceHost.Oracles.ExchangeRates.Terra;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Pylonboard.ServiceHost;

public class PsiPoolArbServiceWorker : IScopedBackgroundServiceWorker
{
    private readonly ILogger<PsiPoolArbServiceWorker> _logger;
    private readonly TerraExchangeRateOracle _exchangeRateOracle;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ArbNotifier _notifier;
    private readonly IEnabledServiceRolesConfig _serviceRolesConfig;

    public PsiPoolArbServiceWorker(
        ILogger<PsiPoolArbServiceWorker> logger,
        TerraExchangeRateOracle exchangeRateOracle,
        IDbConnectionFactory dbConnectionFactory,
        ArbNotifier notifier,
        IEnabledServiceRolesConfig serviceRolesConfig
    )
    {
        _logger = logger;
        _exchangeRateOracle = exchangeRateOracle;
        _dbConnectionFactory = dbConnectionFactory;
        _notifier = notifier;
        _serviceRolesConfig = serviceRolesConfig;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        if (!_serviceRolesConfig.IsRoleEnabled(ServiceRoles.BACKGROUND_WORKER))
        {
            _logger.LogInformation("Background worker role not active, not starting arb bot");
            return;
        }

        do
        {
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
                    await _notifier.HandlePotentialArbAsync(TerraDenominators.bPsiDP, endResult, stoppingToken);
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

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        } while (!stoppingToken.IsCancellationRequested);
    }
}