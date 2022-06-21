namespace SapientFi.ServiceHost.Endpoints.FxRates;

public class FxRateItemGraph
{
    public string From { get; set; } = string.Empty;

    public string To { get; set; } = string.Empty;

    public decimal Converted { get; set; }
}