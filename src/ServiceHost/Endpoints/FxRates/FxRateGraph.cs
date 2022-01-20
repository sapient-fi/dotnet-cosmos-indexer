namespace Pylonboard.ServiceHost.Endpoints.FxRates;

public record FxRateGraph
{
    public List<FxRateItemGraph> Rates { get; set; }
}