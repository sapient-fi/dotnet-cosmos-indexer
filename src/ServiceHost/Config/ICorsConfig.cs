namespace Pylonboard.ServiceHost.Config;

public interface ICorsConfig
{
    List<string> AllowedOrigins { get; }
}