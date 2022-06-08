namespace Sapient.Kernel.Contracts.Exchanges;

public record GetLatestExchangeRateRequest
{
    public string FromDenominator { get; set; }

    public string ToDenominator { get; set; }
}