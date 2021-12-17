using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Pylonboard.ServiceHost.Oracles.ArbNotifier;

public class ArbNotifier
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<ArbNotifier> _logger;
    private readonly ChatId _chatId;
    private ArbDirection _arbDirection;
    private readonly Stopwatch _notificationTimer;

    public ArbNotifier(
        ITelegramBotClient botClient,
        ILogger<ArbNotifier> logger
    )
    {
        _botClient = botClient;
        _logger = logger;
        _chatId = new ChatId(-796208526);
        _notificationTimer = new Stopwatch();
    }

    public async Task HandlePotentialArbAsync(string denom, decimal calculatedArbRateInUst, CancellationToken stoppingToken)
    {
        var breakLevel = 0.95m;
        /**
         * Notification rules:
Quasystaty, [16 Dec 2021 at 21.18.14]:
If under 0.85 then notify once
If 0.85-0.95 do every 30 min or so

If over then again once

Also store this as a time-series then we can visualize the arb-opportunities in the Pylonboard UI
         */

        if (calculatedArbRateInUst < breakLevel)
        {
            await HandleBuyArbAsync(denom, calculatedArbRateInUst, stoppingToken);
            return;
        }

        await HandleSellArbAsync(denom, calculatedArbRateInUst, stoppingToken);
    }

    private async Task HandleSellArbAsync(string denom, decimal calculatedArbRateInUst, CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Denom} sell arb! {ToUst:F} on the dollar", denom, calculatedArbRateInUst);

        if (_arbDirection == ArbDirection.Sell)
        {
            _logger.LogDebug("Not doing anything, last arb was also sell so we have notified already");
            return;

        }
        
        var messageText = $"{denom} SELL arb! {calculatedArbRateInUst:F} on the dollar";
        
        await _botClient.SendTextMessageAsync(
            _chatId, messageText,
            cancellationToken: stoppingToken
        );
        _arbDirection = ArbDirection.Sell;
        
        _notificationTimer.Stop();
        _notificationTimer.Reset();
    }

    private async Task HandleBuyArbAsync(string denom, decimal calculatedArbRateInUst, CancellationToken stoppingToken)
    {
        _logger.LogInformation("{Denom} buy arb! {ToUst:F} on the dollar", denom, calculatedArbRateInUst);
        var messageText = $"{denom} BUY arb! {calculatedArbRateInUst:F} on the dollar";
        if (calculatedArbRateInUst is >= 0.85m and <= 0.95m)
        {
            if (_notificationTimer.Elapsed < TimeSpan.FromMinutes(60) && _notificationTimer.IsRunning)
            {
                _logger.LogDebug("Last notification was {Notify:T} time ago, not notifying now", _notificationTimer.Elapsed);
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
        else if (calculatedArbRateInUst < 0.85m)
        {
            if (_arbDirection == ArbDirection.Buy)
            {
                _logger.LogDebug("Arb direction is already buy, so notification is already sent");
                return;
            }
            await _botClient.SendTextMessageAsync(
                _chatId,
                messageText,
                cancellationToken: stoppingToken
            );
            _arbDirection = ArbDirection.Buy;
        }
    }
}

enum ArbDirection
{
    None,
    
    Buy,
    
    Sell
}