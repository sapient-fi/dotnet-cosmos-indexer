using System.Text.Json;
using NewRelic.Api.Agent;
using Pylonboard.Kernel.IdGeneration;
using Pylonboard.ServiceHost.DAL.TerraMoney;
using Pylonboard.ServiceHost.Oracles;
using Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.TerraDataFetchers;

public class MineBuybackDataFetcher
{
    private readonly ILogger<MineBuybackDataFetcher> _logger;
    private readonly TerraTransactionEnumerator _transactionEnumerator;
    private readonly IdGenerator _idGenerator;
    private readonly IDbConnectionFactory _dbFactory;

    public MineBuybackDataFetcher(
        ILogger<MineBuybackDataFetcher> logger,
        TerraTransactionEnumerator transactionEnumerator,
        IdGenerator idGenerator,
        IDbConnectionFactory dbFactory)
    {
        _logger = logger;
        _transactionEnumerator = transactionEnumerator;
        _idGenerator = idGenerator;
        _dbFactory = dbFactory;
    }

    [Transaction]
    public async Task FetchDataAsync(CancellationToken stoppingToken, bool fullResync = false)
    {
        const long offset = 0;
        var buyBacks = new List<TerraMineBuyBack>();
        using var db = _dbFactory.OpenDbConnection();
        var latestBuyback = await db.SingleAsync(db.From<TerraMineBuybackEntity>()
            .OrderByDescending(q => q.CreatedAt), token: stoppingToken);
        
        await foreach (var (tx, stdTx) in _transactionEnumerator.EnumerateTransactionsAsync(
                           offset,
                           100,
                           TerraStakingContracts.MINE_BUYBACK_CONTRACT,
                           stoppingToken
                       ))
        {
            if (tx.Id == latestBuyback?.TransactionId)
            {
                _logger.LogDebug("Mine buyback exists already for tx: {TxHash}", tx.TransactionHash);
                break;
            }
            using var dbTx = db.BeginTransaction();
            await dbTx.Connection.SaveAsync(obj: new TerraRawTransactionEntity
                {
                    Id = tx.Id,
                    CreatedAt = tx.CreatedAt,
                    TxHash = tx.TransactionHash,
                    RawTx = JsonSerializer.Serialize(tx),
                },
                token: stoppingToken
            );
            
            var events = tx.Logs
                .SelectMany(l => l.Events)
                .Where(evt =>
                    evt.Attributes.Contains(new TxLogEventAttribute() { Key = "action", Value = "sweep" }) &&
                    evt.Type.EqualsIgnoreCase("from_contract")
                ).ToList();

            if (!events.Any())
            {
                continue;
            }

            foreach (var evt in events)
            {
                _logger.LogDebug("Have sweep event: {Event}", evt);
                var distributeMineAmountStr =
                    evt.Attributes.First(attrib => 
                        attrib.Key.EqualsIgnoreCase("distribute_amount")).Value;
                var mineAmount = distributeMineAmountStr.ToInt64();
                var offerAmountUstStr =
                    evt.Attributes.First(attribute =>
                        attribute.Key.EqualsIgnoreCase("offer_amount")).Value;
                var offerAmountUst = offerAmountUstStr.ToInt64() / 1_000_000m;
                
                buyBacks.Add(new TerraMineBuyBack
                {
                    TxId = tx.Id,
                    TxHash = tx.TransactionHash,
                    AmountInU = mineAmount,
                    UstAmount = offerAmountUst,
                    CreatedAt = tx.CreatedAt,
                });
            }

            dbTx.Commit();
        }

        // Now we have the new buybacks. Sort them in the appropriate order and go to town
        var sortedBuyBacks = buyBacks.OrderBy(buyBack => buyBack.CreatedAt);
        foreach (var buyBack in sortedBuyBacks)
        {
            _logger.LogInformation("Distributing buy-backs for sweep {TxHash} w. total MINE amount {TerraAmount}",
                buyBack.TxHash, buyBack.AmountInU / 1_000_000m);
            using var dbTx = db.BeginTransaction();
            var query = db.From<TerraMineStakingEntity>()
                .Where(q => q.CreatedAt <= buyBack.CreatedAt)
                .GroupBy(entity => entity.Sender)
                .OrderBy("2")
                .Select(x => new { x.Sender, StakedSum = Sql.Sum("amount") });
            var stakingResults =
                await dbTx.Connection.DictionaryAsync<string, decimal>(query, token: stoppingToken);
            var totalStakingSum = stakingResults.Sum(pair => pair.Value);
            _logger.LogDebug("Total staked at time {Time} was {StakingAmount}", buyBack.CreatedAt, totalStakingSum);
                
            foreach (var staker in stakingResults.Where(pair => pair.Value > 0.0m))
            {
                var stakersShare = ((staker.Value / totalStakingSum) * buyBack.AmountInU) / 1_000_000m;

                await db.InsertAsync(new TerraMineStakingEntity
                {
                    Id = _idGenerator.Snowflake(),
                    TransactionId = buyBack.TxId,
                    TxHash = buyBack.TxHash,
                    Amount = stakersShare,
                    Sender = staker.Key,
                    CreatedAt = buyBack.CreatedAt,
                    IsBuyBack = true,
                }, token: stoppingToken);
            }

            await db.InsertAsync(new TerraMineBuybackEntity
            {
                Id = _idGenerator.Snowflake(),
                CreatedAt = buyBack.CreatedAt,
                MineAmount = buyBack.AmountInU / 1_000_000M,
                UstAmount = buyBack.UstAmount,
                TransactionHash = buyBack.TxHash,
                TransactionId = buyBack.TxId,
            }, token: stoppingToken);
            dbTx.Commit();
            _logger.LogDebug("Done distributing buy-backs for sweep {TxHash} w. total MINE amount", buyBack.TxHash);
        }
    }
}

public record TerraMineBuyBack
{
    public long TxId { get; set; }
    public string TxHash { get; set; }
    public decimal UstAmount { get; set; }
    public decimal AmountInU { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}