using NewRelic.Api.Agent;
using Pylonboard.Kernel.DAL.Entities.Terra.Views;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.Endpoints.MyPylonStake;

public class MyPylonStakeService
{
    private readonly ILogger<MyPylonStakeService> _logger;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MyPylonStakeService(
        ILogger<MyPylonStakeService> logger,
        IDbConnectionFactory dbConnectionFactory
    )
    {
        _logger = logger;
        _dbConnectionFactory = dbConnectionFactory;
    }

    [Trace]
    public async Task<MyPylonStakeGraph?> GetMyPylonStakeAsync(string terraWallet, CancellationToken cancellationToken)
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(token: cancellationToken);

        var staked = await db.SingleAsync<decimal>(
            db.From<MineWalletStakeView>()
                .Where(q => q.Wallet == terraWallet)
                .Select(q => new
                {
                    Amount = q.Sum
                }), token: cancellationToken);

        return new MyPylonStakeGraph
        {
            Amount = staked
        };
    }
}