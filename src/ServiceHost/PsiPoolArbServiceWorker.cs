using Pylonboard.Kernel;
using Pylonboard.Kernel.Hosting.BackgroundWorkers;
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

    public PsiPoolArbServiceWorker(
        ILogger<PsiPoolArbServiceWorker> logger,
        TerraExchangeRateOracle exchangeRateOracle,
        IDbConnectionFactory dbConnectionFactory,
        ArbNotifier notifier
    )
    {
        _logger = logger;
        _exchangeRateOracle = exchangeRateOracle;
        _dbConnectionFactory = dbConnectionFactory;
        _notifier = notifier;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        do
        {
            var now = DateTimeOffset.Now;
            var toPsi = await _exchangeRateOracle.GetExchangeRateAsync(TerraDenominators.bPsiDP, TerraDenominators.Psi, now);
            var toUst = await _exchangeRateOracle.GetExchangeRateAsync(TerraDenominators.Psi, TerraDenominators.Ust, now);

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
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        } while (!stoppingToken.IsCancellationRequested);
    }
}