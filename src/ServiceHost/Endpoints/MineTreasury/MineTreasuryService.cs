using System.Data;
using NewRelic.Api.Agent;
using Pylonboard.Kernel.DAL.Entities.Terra;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.Endpoints.MineTreasury;

public class MineTreasuryService
{
    private readonly ILogger<MineTreasuryService> _logger;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MineTreasuryService(
        ILogger<MineTreasuryService> logger,
        IDbConnectionFactory dbConnectionFactory
    )
    {
        _logger = logger;
        _dbConnectionFactory = dbConnectionFactory;
    }
    
    [Trace]
    public async Task<MineTreasuryGraph> GetTreasuryOverviewAsync(CancellationToken cancellationToken)
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(cancellationToken);

        var results = new MineTreasuryGraph
        {
            Buybacks = await GetBuybacksAsync(db, cancellationToken)
        };

        return results;
    }

    [Trace]
    private async Task<List<MineBuybackGraph>> GetBuybacksAsync(IDbConnection db, CancellationToken cancellationToken)
    {
        var data = await db.SelectAsync<MineBuybackGraph>(
            db.From<TerraMineBuybackEntity>()
                .OrderByDescending(entity => entity.CreatedAt)
                .Select(entity => new 
                {
                    CreatedAt = entity.CreatedAt,
                    MineAmount = entity.MineAmount,
                    TransactionHash = entity.TransactionHash,
                    UstAmount = entity.UstAmount
                }), token: cancellationToken);

        return data;
    }

    [Trace]
    public async Task<IEnumerable<MineBuybackGraph>> GetBuybackByWalletAsync(string wallet, CancellationToken cancellationToken)
    {
        using var db = await _dbConnectionFactory.OpenDbConnectionAsync(cancellationToken);
        var data = await db.SelectAsync<MineBuybackGraph>(
            db.From<TerraMineStakingEntity>()
                .Where(entity => entity.Sender == wallet && entity.IsBuyBack == true)
                .OrderByDescending(entity => entity.CreatedAt)
                .Select(entity => new
                {
                    CreatedAt = entity.CreatedAt,
                    MineAmount = entity.Amount,
                    TransactionHash = entity.TxHash,
                    UstAmount = 0
                }), token: cancellationToken);
        return data;
    }
}