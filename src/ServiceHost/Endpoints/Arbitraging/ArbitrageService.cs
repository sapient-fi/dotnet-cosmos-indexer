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

    public async Task<List<TimeSeriesStatEntry>> GetArbTimeSeriesForMarketAsync(ArbitrageMarket market, CancellationToken cancellationToken)
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
}