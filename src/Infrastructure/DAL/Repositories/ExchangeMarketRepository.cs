//TODO do we still need this?
using Microsoft.Extensions.Logging;
using ServiceStack.Data;

namespace SapientFi.Infrastructure.DAL.Repositories;

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
}