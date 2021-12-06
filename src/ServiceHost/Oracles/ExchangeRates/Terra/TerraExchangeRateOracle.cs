using Pylonboard.Kernel;
using Pylonboard.ServiceHost.Oracles.ExchangeRates.Terra.LowLevel;
using ServiceStack;
using ServiceStack.Data;

namespace Pylonboard.ServiceHost.Oracles.ExchangeRates.Terra;

public class TerraExchangeRateOracle
{
    private readonly ITerraMoneyExchangeRateApiClient _exchangeRateApiClient;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<TerraExchangeRateOracle> _logger;

    public TerraExchangeRateOracle(
        ITerraMoneyExchangeRateApiClient exchangeRateApiClient,
        IDbConnectionFactory connectionFactory,
        ILogger<TerraExchangeRateOracle> logger
    )
    {
        _exchangeRateApiClient = exchangeRateApiClient;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get the exchange rate of a currency pair, using the rate as it was <paramref name="atTime"/> 
    /// </summary>
    /// <param name="fromDenom"></param>
    /// <param name="toDenom"></param>
    /// <param name="atTime"></param>
    public async Task<(decimal close, DateTimeOffset closedAt)> GetExchangeRateAsync(string fromDenom, string toDenom, DateTimeOffset atTime)
    {
        if (fromDenom.EqualsIgnoreCase(TerraDenominators.Ust))
        {
            return (1m, atTime);
        }
            
        if (!toDenom.EqualsIgnoreCase(TerraDenominators.Ust))
        {
            throw new NotSupportedException("Currently only supports converting _to_ UST");
        }
            
        var contractAddr = fromDenom switch
        {
            TerraDenominators.Luna => "terra1tndcaqxkpc5ce9qee5ggqf430mr2z3pefe5wj6",
            TerraDenominators.nLuna => "terra1tndcaqxkpc5ce9qee5ggqf430mr2z3pefe5wj6", // Handle nLuna as LUNA as it's basically the same
            TerraDenominators.Anc => "terra1gm5p3ner9x9xpwugn9sp6gvhd0lwrtkyrecdn3",
            TerraDenominators.Mine => "terra178jydtjvj4gw8earkgnqc80c3hrmqj4kw2welz",
            TerraDenominators.Mir => "terra1amv303y8kzxuegvurh0gug2xe9wkgj65enq2ux",
            TerraDenominators.Loop => "terra106a00unep7pvwvcck4wylt4fffjhgkf9a0u6eu",
            TerraDenominators.Vkr => "terra1e59utusv5rspqsu8t37h5w887d9rdykljedxw0",
            TerraDenominators.Stt => "terra19pg6d7rrndg4z4t0jhcd7z9nhl3p5ygqttxjll",
            TerraDenominators.Psi => "terra163pkeeuwxzr0yhndf8xd2jprm9hrtk59xf7nqf",
            TerraDenominators.Twd => "terra1etdkg9p0fkl8zal6ecp98kypd32q8k3ryced9d",
            TerraDenominators.Loopr => "terra1dw5j23l6nwge69z0enemutfmyc93c36aqnzjj5",
            TerraDenominators.Orion => "terra1z6tp0ruxvynsx5r9mmcc2wcezz9ey9pmrw5r8g",
            TerraDenominators.Apollo => "terra1xj2w7w8mx6m2nueczgsxy2gnmujwejjeu2xf78",
            TerraDenominators.bEth => "terra1c0afrdc5253tkp5wt7rxhuj42xwyf2lcre0s7c",
            TerraDenominators.nEth => "terra1c0afrdc5253tkp5wt7rxhuj42xwyf2lcre0s7c", // Lookup nEth as bEth as it's basically the same
            _ => throw new ArgumentOutOfRangeException(nameof(fromDenom), $"No contract for {fromDenom}")
        };
            
        var response = await _exchangeRateApiClient.GetExchangeRateAsync(
            atTime.ToUnixTimeSeconds(),
            atTime.AddMinutes(15).ToUnixTimeSeconds(),
            contractAddr
        );

        var first = response.Content.First();

        if (first.Close <= 0.0000001m)
        {
            first.Close *= 1_000_000m;

            if (first.Close <= 0.0000001m)
            {
                throw new OperationCanceledException(
                    $"Close value for market {fromDenom}-{toDenom} is {first.Close:F2}");
            }
        }
            
        return (first.Close, DateTimeOffset.FromUnixTimeMilliseconds(first.Time));
    }
}