namespace SapientFi.ServiceHost.Endpoints.FxRates;

public class FxRateQuery
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}