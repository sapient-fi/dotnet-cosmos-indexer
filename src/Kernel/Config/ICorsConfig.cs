namespace Sapient.Kernel.Config;

public interface ICorsConfig
{
    List<string> AllowedOrigins { get; }
}