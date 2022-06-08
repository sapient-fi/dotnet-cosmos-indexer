using Microsoft.Extensions.Logging;
using Sapient.Kernel;
using Sapient.Kernel.DAL.Entities.Exchanges;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Sapient.Infrastructure.DAL.Repositories;

public class ExchangeMarketRepository
{
    private readonly ILogger<ExchangeMarketRepository> _logger;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public ExchangeMarketRepository(
        ILogger<ExchangeMarketRepository> logger,
        IDbConnectionFactory dbConnectionFactory
    )
    {
        _logger = logger;
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<ExchangeMarketCandle> GetLatestRateAsync(
        string market, 
        Exchange exchange,
        CancellationToken cancellationToken
    )
    {
        _logger.LogDebug("querying db for rates on market {Market} from exchange {Exchange}", market, exchange);
        
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);

        var rate = await db.SingleAsync(
            db.From<ExchangeMarketCandle>()
                .Where(q => q.Market == market && q.Exchange == exchange)
                .OrderByDescending(q => q.CloseTime)
                .Take(1),
            token: cancellationToken);

        return rate;
    }
}