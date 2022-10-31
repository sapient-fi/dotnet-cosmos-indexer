using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using NewRelic.Api.Agent;
using SapientFi.ServiceHost.Endpoints.FxRates;

namespace SapientFi.ServiceHost.Endpoints;

public class Query
{
    [Trace]
    public async Task<FxRateGraph> GetFxRates(
        FxRateQuery[] rates,
        [Service] FxRatesService fxRatesService,
        CancellationToken cancellationToken
    )
    {
        var results = await fxRatesService.ConvertEmAllAsync(rates, cancellationToken);

        return new FxRateGraph
        {
            Rates = results,
        };
    }
}