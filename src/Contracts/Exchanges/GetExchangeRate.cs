namespace Pylonboard.Contracts.Exchanges;

public class GetExchangeRate
{
    public string FromDenominator { get; set; }

    public string ToDenominator { get; set; }

    public DateTimeOffset AtTime { get; set; }
}