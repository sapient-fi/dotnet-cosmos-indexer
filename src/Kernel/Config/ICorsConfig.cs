using System.Collections.Generic;

namespace SapientFi.Kernel.Config;

public interface ICorsConfig
{
    List<string> AllowedOrigins { get; }
}