namespace Pylonboard.Kernel.Contracts.Exchanges;

public class GetExchangeRateResult
{
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public DateTimeOffset ClosedAt { get; set; }
}