using Pylonboard.Kernel;
using Pylonboard.ServiceHost.DAL.Exchanges;
using Pylonboard.ServiceHost.Endpoints.Types;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.Endpoints.Arbitraging;

public class ArbitrageService
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<ArbitrageService> _service;

    public ArbitrageService(
        IDbConnectionFactory dbConnectionFactory,
        ILogger<ArbitrageService> service
    )
    {
        _dbConnectionFactory = dbConnectionFactory;
        _service = service;
    }

    public async Task<List<TimeSeriesStatEntry>> GetArbTimeSeriesForMarketAsync(ArbitrageMarket market,
        CancellationToken cancellationToken)
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);

        /*
         *  SELECT time_bucket('{bucket}', time) AS bucket,
    last(price_close, time) AS last_closing_price
    FROM stocks_intraday
    WHERE symbol = '{symbol}'
    GROUP BY bucket
    ORDER BY bucket
""".format(bucket="7 days", symbol="AAPL")
         */
        var query = db.From<ExchangeMarketCandle>()
            .Select(x => new
            {
                At = Sql.Custom("time_bucket('1 hour', close_time)"),
                Value = Sql.Avg(x.Close)
            })
            .Where(x => x.Exchange == Exchange.Terra && x.Market == "bPsiDP-arb") // TODO dynamic from `market`!! 
            .GroupBy("at")
            .OrderBy("at");

        var results = await db.SqlListAsync<TimeSeriesStatEntry>(query, token: cancellationToken);

        return results;
    }

    public async Task<List<ArbBands>> GetArbitrageTimeSeriesBandsAsync(
        ArbitrageMarket market,
        CancellationToken cancellationToken
    )
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);

        var results = await db
            .SqlListAsync<ArbBands>(
                @"
select s.close_time,
        s.close,
        close_smooth * 1.0-(@percentage/2) as lower_band,
        close_smooth * 1.0+(@percentage/2) as upper_band
 FROM (SELECT close_time,
              close,
              AVG(close) OVER (ORDER BY close_time
                  ROWS BETWEEN @periods PRECEDING AND CURRENT ROW)
                  AS close_smooth
       FROM exchange_market_candle
       WHERE market = @market
         and close_time > NOW() - INTERVAL '1 day'
       ORDER BY close_time DESC) s LIMIT @periods;", new
                {
                    percentage = 0.05m,
                    periods = 161,
                    market = $"{TerraDenominators.bPsiDP}-arb",
                }, token: cancellationToken);

        return results;
    }
}