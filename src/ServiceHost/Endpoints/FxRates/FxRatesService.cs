using MassTransit;
using Pylonboard.ServiceHost.Consumers;

namespace Pylonboard.ServiceHost.Endpoints.FxRates;

public class FxRatesService
{
    private readonly ILogger<FxRatesService> _logger;
    private readonly IRequestClient<GetLatestExchangeRateRequest> _fxRateClient;

    public FxRatesService(
        ILogger<FxRatesService> logger,
        IRequestClient<GetLatestExchangeRateRequest> fxRateClient
    )
    {
        _logger = logger;
        _fxRateClient = fxRateClient;
    }

    public async Task<List<FxRateItemGraph>> ConvertEmAllAsync(FxRateQuery[] rates, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling convert em all request");
        var results = new List<FxRateItemGraph>();

        foreach (var rate in rates)
        {
            _logger.LogDebug("Converting form {FromDenom} to {ToDenom}", rate.From, rate.To);
            var result = await _fxRateClient.GetResponse<GetLatestExchangeRateResult>(new GetLatestExchangeRateRequest
            {
                FromDenominator = rate.From,
                ToDenominator = rate.To
            }, cancellationToken);

            results.Add(new FxRateItemGraph
            {
                From = rate.From,
                To = rate.To,
                Converted = rate.Amount * result.Message.Value
            });
        }

        return results;
    }
}