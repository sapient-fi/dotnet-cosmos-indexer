using Pylonboard.ServiceHost.Oracles.ExchangeRates.Fiat.LowLevel;

namespace Pylonboard.ServiceHost.Oracles.ExchangeRates.Fiat;

public class FiatExchangeRateOracle
{
    private readonly IOpenExchangeRateApiClient _client;
    private readonly ILogger<FiatExchangeRateOracle> _logger;

    public FiatExchangeRateOracle(
        IOpenExchangeRateApiClient client,
        ILogger<FiatExchangeRateOracle> logger
    )
    {
        _client = client;
        _logger = logger;
    }

    public async Task<FiatApiClientResponse> GetExchangeRatesAsync(DateTimeOffset at)
    {
        var data = await _client.GetExchangeRatesAsync(
            at.Year.ToString(),
            at.Month.ToString("D2"),
            at.Day.ToString("D2"),
            "4a9a456f06a942a78ab807bb7e35d181"
        );

        if (data.Error != null)
        {
            var error = data.Error;
            _logger.LogCritical(
                "Error during call to OpenExchangeRates: status {Status} message {Message} - Zen: {Zen}",
                error.StatusCode, 
                error.Message, 
                error.Content
            );
            throw new OperationCanceledException(
                $"Error during external call. {error.StatusCode} with {error.Message}"
            );
        }
            
        return new FiatApiClientResponse
        {
            At = new DateTimeOffset(at.Year, at.Month, at.Day, 0, 0, 0, TimeSpan.Zero),
            Rates = data.Content.Rates.Select(pair => new FiatExchangeRate
            {
                Rate = 1/pair.Value, // we need the multiplicative inverse
                Denominator = pair.Key,
                Base = data.Content.Base
            })
        };
    }
}

public record FiatExchangeRate
{
    public decimal Rate { get; set; }
    public string Denominator { get; set; }
        
    public string Base { get; set; }
}

public record FiatApiClientResponse
{
    public DateTimeOffset At { get; set; }
    public IEnumerable<FiatExchangeRate> Rates { get; set; }
}