using System.Collections.Generic;

namespace SapientFi.ServiceHost.Endpoints.FxRates;

public record FxRateGraph
{
    public List<FxRateItemGraph> Rates { get; set; } = new();
}