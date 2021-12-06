using Refit;

namespace Pylonboard.ServiceHost.Oracles.ExchangeRates.Fiat.LowLevel;

public interface IOpenExchangeRateApiClient
{
    [Get("/api/historical/{year}-{month}-{day}.json")]
    public Task<ApiResponse<OpenExchangeRateResponse>> GetExchangeRatesAsync(
        string year,
        string month,
        string day,
        [AliasAs("app_id")] string accessKey
    );
}