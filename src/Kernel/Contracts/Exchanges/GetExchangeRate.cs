namespace SapientFi.Kernel.Contracts.Exchanges;

public class GetExchangeRate
{
    public string FromDenominator { get; set; } = string.Empty;

    public string ToDenominator { get; set; } = string.Empty;

    public DateTimeOffset AtTime { get; set; }
}