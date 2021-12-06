using System.Threading.Tasks;
using Refit;

namespace Invacoil.ServiceRole.Oracle.ExchangeRates.Fiat.LowLevel
{
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
}