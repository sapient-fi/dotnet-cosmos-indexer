using System.Diagnostics;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.Endpoints.Arbitraging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Pylonboard.ServiceHost.Oracles.ArbNotifier;

public class ArbNotifier
{
    private readonly ITelegramBotClient _botClient;
    private readonly ArbitrageService _arbitrageService;
    private readonly ITelegramConfig _telegramConfig;
    private readonly ILogger<ArbNotifier> _logger;
    private readonly ChatId _chatId;
    private ArbDirection _arbDirection;
    private readonly Stopwatch _notificationTimer;

    public ArbNotifier(
        ITelegramBotClient botClient,
        ArbitrageService arbitrageService,
        ITelegramConfig telegramConfig,
        ILogger<ArbNotifier> logger
    )
    {
        _botClient = botClient;
        _arbitrageService = arbitrageService;
        _telegramConfig = telegramConfig;
        _logger = logger;
        _chatId = new ChatId(long.Parse(telegramConfig.ChatId));
        _notificationTimer = new Stopwatch();
    }

    public async Task HandlePotentialArbAsync(
        string denom,
        decimal calculatedArbRateInUst,
        CancellationToken stoppingToken
    )
    {
        // Notification rules...
        // We're computing a band of +/- 2.5% around the 161 periods average of close prices on the arb... if the current
        // price has broken below the lower-band, we have a  buy arb
        // if the price has broken above the upper-band, we have a sell arb
        var priceBands = await _arbitrageService.GetArbitrageTimeSeriesBandsAsync(ArbitrageMarket.Nexus, stoppingToken);
        var relevantBand = priceBands.Last();

        if (calculatedArbRateInUst < relevantBand.LowerBand)
        {
            await HandleBuyArbAsync(denom, calculatedArbRateInUst, relevantBand.LowerBand, stoppingToken);
            
            return;
        }

        if (calculatedArbRateInUst > relevantBand.UpperBand)
        {
            await HandleSellArbAsync(denom, calculatedArbRateInUst, relevantBand.UpperBand, stoppingToken);

            return;
        }
        // send a pulse notification to let the bot know that new data is there
        // as it might have relevant positions to close
        _logger.LogInformation("Sending out pulse arb on WebSocket");
    }

    private async Task HandleSellArbAsync(
        string denom,
        decimal calculatedArbRateInUst,
        decimal upperBand,
        CancellationToken stoppingToken
    )
    {
        _logger.LogInformation("{Denom} sell arb! {ToUst:F4} crossed up over {UpperBand:F4}", denom,
            calculatedArbRateInUst, upperBand);

        if (_arbDirection == ArbDirection.Sell)
        {
            _logger.LogDebug("Not doing anything, last arb was also sell so we have notified already");
            return;
        }

        var messageText =
            $"{denom} SELL arb! {calculatedArbRateInUst:F4} crossed up over {upperBand:F4} (131 period avg)";

        await _botClient.SendTextMessageAsync(
            _chatId, messageText,
            cancellationToken: stoppingToken
        );
        _arbDirection = ArbDirection.Sell;

        _notificationTimer.Stop();
        _notificationTimer.Reset();
    }

    private async Task HandleBuyArbAsync(
        string denom,
        decimal calculatedArbRateInUst,
        decimal lowerBand,
        CancellationToken stoppingToken
    )
    {
        _logger.LogInformation("{Denom} buy arb! {ToUst:F4} crossed down under {UpperBand:F4}", denom,
            calculatedArbRateInUst, lowerBand);
        var messageText =
            $"{denom} BUY arb!  {calculatedArbRateInUst:F4} crossed down under {lowerBand:F4} (131 period avg)";

        if (_notificationTimer.Elapsed < TimeSpan.FromHours(4) && _notificationTimer.IsRunning &&
            _arbDirection == ArbDirection.Buy)
        {
            _logger.LogDebug("Last notification was {Notify:T} time ago, not notifying now",
                _notificationTimer.Elapsed);
            return;
        }

        _notificationTimer.Stop();
        _notificationTimer.Reset();

        await _botClient.SendTextMessageAsync(
            _chatId,
            messageText,
            cancellationToken: stoppingToken
        );
        _arbDirection = ArbDirection.Buy;

        _notificationTimer.Start();
    }
}

enum ArbDirection
{
    None,

    Buy,

    Sell,
    
    Pulse
}