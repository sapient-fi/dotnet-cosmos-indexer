namespace Pylonboard.ServiceHost.Endpoints.FxRates;

public class FxRateQuery
{
    public string From { get; set; }
    public string To { get; set; }
    public decimal Amount { get; set; }
}