using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace Invacoil.ServiceRole.Oracle.ExchangeRates.Terra.LowLevel
{
    public interface ITerraMoneyExchangeRateApiClient
    {
        // https://api.coinhall.org/api/v1/charts/terra/candles?bars=320&from=1634819556&interval=1m&pairAddress=terra1tndcaqxkpc5ce9qee5ggqf430mr2z3pefe5wj6&quoteAsset=uusd&to=1634838756
        [Get("/api/v1/charts/terra/candles")]
        public Task<ApiResponse<List<TerraMoneyExchangeRateResponse>>> GetExchangeRateAsync(
            long from,
            long to,
            string pairAddress,
            string quoteAsset = "uusd",
            string interval = "1m"
        );
    }
}