namespace Pylonboard.ServiceHost.Config;

public interface ITelegramConfig
{
    string TelegramBotToken { get; }
    
    string ChatId { get; }
}