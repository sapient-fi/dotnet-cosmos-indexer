namespace SapientFi.Kernel.Contracts.Exchanges;

public record GetLatestExchangeRateRequest
{
    public string FromDenominator { get; set; } = string.Empty;

    public string ToDenominator { get; set; } = string.Empty;
}