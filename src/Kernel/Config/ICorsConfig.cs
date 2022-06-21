namespace SapientFi.Kernel.Config;

public interface ICorsConfig
{
    List<string> AllowedOrigins { get; }
}