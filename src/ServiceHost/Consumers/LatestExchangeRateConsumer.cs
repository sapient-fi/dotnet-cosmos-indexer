using System.Text.RegularExpressions;
using HotChocolate.Utilities;
using MassTransit;
using Pylonboard.Kernel;
using Pylonboard.ServiceHost.DAL.Exchanges;
using Pylonboard.ServiceHost.Oracles.ExchangeRates.Fiat;
using Pylonboard.ServiceHost.Oracles.ExchangeRates.Terra;
using ServiceStack.Caching;
using ServiceStack.Data;

namespace Pylonboard.ServiceHost.Consumers;

public class LatestExchangeRateConsumer : IConsumer<GetLatestExchangeRateRequest>
{
    private readonly ILogger<LatestExchangeRateConsumer> _logger;
    private readonly TerraExchangeRateOracle _terraOracle;
    // private readonly FiatExchangeRateOracle _fiatOracle;
    private readonly ExchangeMarketRepository _repository;
    private readonly ICacheClient _cacheClient;

    public LatestExchangeRateConsumer(
        ILogger<LatestExchangeRateConsumer> logger,
        TerraExchangeRateOracle terraOracle,
        // FiatExchangeRateOracle fiatOracle,
        ExchangeMarketRepository repository,
        ICacheClient cacheClient
    )
    {
        _logger = logger;
        _terraOracle = terraOracle;
        // _fiatOracle = fiatOracle;
        _repository = repository;
        _cacheClient = cacheClient;
    }

    public async Task Consume(ConsumeContext<GetLatestExchangeRateRequest> context)
    {
        var request = context.Message;

        // if (IsFiat(request.FromDenominator))
        // {
        //     throw new NotImplementedException("FIAT denoms not supported yet");
        // }

        var response =
            await HandleTerraRateAsync(request.FromDenominator.ToUpperInvariant(),
                request.ToDenominator.ToUpperInvariant(), context.CancellationToken);

        await context.RespondAsync(response);
    }

    private async Task<GetLatestExchangeRateResult> HandleTerraRateAsync(
        string fromDenominator,
        string toDenominator,
        CancellationToken cancellationToken
    )
    {
        var market = CreateMarketFromDenoms(fromDenominator, toDenominator);
        var cacheKey = $"cache:fx:terra:{market}";
        var cached = _cacheClient.Get<GetLatestExchangeRateResult>(cacheKey);

        if (cached != default)
        {
            return cached;
        }

        // No hot cache, look in the db
        var rate = await _repository.GetLatestRateAsync(market, Exchange.Terra, cancellationToken);

        if (rate == default)
        {
            throw new OperationCanceledException($"Unable to find rate for {market}");
        }

        var result = new GetLatestExchangeRateResult
        {
            Value = rate.Close,
            ClosedAt = rate.CloseTime,
        };
        _cacheClient.Set(cacheKey, result, TimeSpan.FromHours(1));

        return result;
    }

    private string CreateMarketFromDenoms(string fromDenominator, string toDenominator)
    {
        return fromDenominator.EqualsInvariantIgnoreCase(TerraDenominators.Ust)
            ? $"{toDenominator}-${fromDenominator}"
            : $"{fromDenominator}-{toDenominator}";
    }

    private bool IsFiat(string denominator)
    {
        var fiatRegex = new Regex(
            "^(AED|AFN|ALL|AMD|ANG|AOA|ARS|AUD|AWG|AZN|BAM|BBD|BDT|BGN|BHD|BIF|BMD|BND|BOB|BRL|BSD|BTC|BTN|BWP|BYN|BZD|CAD|CDF|CHF|CLF|CLP|CNH|CNY|COP|CRC|CUC|CUP|CVE|CZK|DJF|DKK|DOP|DZD|EGP|ERN|ETB|EUR|FJD|FKP|GBP|GEL|GGP|GHS|GIP|GMD|GNF|GTQ|GYD|HKD|HNL|HRK|HTG|HUF|IDR|ILS|IMP|INR|IQD|IRR|ISK|JEP|JMD|JOD|JPY|KES|KGS|KHR|KMF|KPW|KRW|KWD|KYD|KZT|LAK|LBP|LKR|LRD|LSL|LYD|MAD|MDL|MGA|MKD|MMK|MNT|MOP|MRO|MRU|MUR|MVR|MWK|MXN|MYR|MZN|NAD|NGN|NIO|NOK|NPR|NZD|OMR|PAB|PEN|PGK|PHP|PKR|PLN|PYG|QAR|RON|RSD|RUB|RWF|SAR|SBD|SCR|SDG|SEK|SGD|SHP|SLL|SOS|SRD|SSP|STD|STN|SVC|SYP|SZL|THB|TJS|TMT|TND|TOP|TRY|TTD|TWD|TZS|UAH|UGX|USD|UYU|UZS|VES|VND|VUV|WST|XAF|XAG|XAU|XCD|XDR|XOF|XPD|XPF|XPT|YER|ZAR|ZMW|ZWL|SDR)$",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        return fiatRegex.IsMatch(denominator);
    }
}

public record GetLatestExchangeRateRequest
{
    public string FromDenominator { get; set; }

    public string ToDenominator { get; set; }
}

public record GetLatestExchangeRateResult
{
    public decimal Value { get; set; }
    public DateTimeOffset ClosedAt { get; set; }
}