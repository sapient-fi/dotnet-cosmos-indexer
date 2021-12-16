using Pylonboard.Kernel;
using Pylonboard.Kernel.Hosting.BackgroundWorkers;
using Pylonboard.ServiceHost.Oracles.ArbNotifier;
using Pylonboard.ServiceHost.Oracles.ExchangeRates.Terra;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Pylonboard.ServiceHost;

public class PsiPoolArbServiceWorker : IScopedBackgroundServiceWorker
{
    private readonly ILogger<PsiPoolArbServiceWorker> _logger;
    private readonly TerraExchangeRateOracle _exchangeRateOracle;
    private readonly ArbNotifier _notifier;

    public PsiPoolArbServiceWorker(
        ILogger<PsiPoolArbServiceWorker> logger,
        TerraExchangeRateOracle exchangeRateOracle,
        ArbNotifier notifier
    )
    {
        _logger = logger;
        _exchangeRateOracle = exchangeRateOracle;
        _notifier = notifier;
    }

    public async Task DoWorkAsync(CancellationToken stoppingToken)
    {
        do
        {
            var now = DateTimeOffset.Now;
            var toPsi = await _exchangeRateOracle.GetExchangeRateAsync(TerraDenominators.bPsiDP, TerraDenominators.Psi, now);
            var toUst = await _exchangeRateOracle.GetExchangeRateAsync(TerraDenominators.Psi, TerraDenominators.Ust,
                now);

            var endResult = toUst.close * toPsi.close;
            await _notifier.HandlePotentialArbAsync(TerraDenominators.bPsiDP, endResult, stoppingToken);
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        } while (!stoppingToken.IsCancellationRequested);
    }
}