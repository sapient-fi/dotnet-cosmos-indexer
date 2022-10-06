using System.Net;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using NewRelic.Api.Agent;
using Polly;
using Refit;
using TerraDotnet.TerraLcd;
using TerraDotnet.TerraLcd.Messages;

namespace TerraDotnet;

public class CosmosTransactionEnumerator<T>  
{
    private readonly ILogger<CosmosTransactionEnumerator<T>> _logger;
    private readonly ICosmosLcdApiClient<T> _cosmosClient;

    private const int SecondsPerBlock = 6;
    
    public CosmosTransactionEnumerator(
        ILogger<CosmosTransactionEnumerator<T>> logger,
        ICosmosLcdApiClient<T> cosmosClient
    )
    {
        _logger = logger;
        _cosmosClient = cosmosClient;
    }

    [Trace]
    public async IAsyncEnumerable<LcdTxResponse> EnumerateTransactionsAsync(
        int fromAndIncludingBlockHeight,
        [EnumeratorCancellation] CancellationToken stoppingToken
    )
    {
        var queryWindow = new QueryWindow
        {
            WindowBlockWidth = 2000,
            StartBlockHeight = fromAndIncludingBlockHeight,
            PaginationLimit = 100,
            PaginationOffset = 0
        };
        
        // Fetch the latest block in the chain before we start enumerating. Will be used later.
        var latestBlockResponse = await _cosmosClient.GetLatestBlockAsync();
        if (!latestBlockResponse.IsSuccessStatusCode)
        {
            _logger.LogCritical(latestBlockResponse.Error, "Unable to request latest block, abort");
            throw new Exception("latest block request went wrong... figure it out", latestBlockResponse.Error);
        }
        var latestBlockHeight = latestBlockResponse.Content.Block.Header.HeightAsInt;

        while (true)
        {
            var response = await Policy
                .Handle<ApiException>(exception => exception.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(10, retryCounter => TimeSpan.FromMilliseconds(Math.Pow(10, retryCounter)),
                    (_, span) =>
                    {
                        _logger.LogWarning("Handling retry while enumerating Cosmos Transactions, waiting {Time:c}", span);
                    }
                )
                .ExecuteAsync(
                    async () => await _cosmosClient.GetTransactionsMatchingQueryAsync(
                        queryWindow.GetConditions(),
                        queryWindow.PaginationLimit,
                        queryWindow.PaginationOffset
                    )
                );


            if (response.Pagination.TotalAsInt == 0)
            {
                // now we have to figure out if we went past the
                // latest block, or whether we just hit a "dead period"
                if (queryWindow.EndBlockHeight >= latestBlockHeight)
                {
                    // we should wait a bit for some blocks to be formed
                    await Task.Delay(TimeSpan.FromSeconds(queryWindow.WindowBlockWidth * SecondsPerBlock), stoppingToken);
                    // do NOT advance the query window in this scenario, as we want to
                    // retry the same window later
                    continue;
                }
            }
            else
            {
                foreach (var txResponse in response.TransactionResponses)
                {
                    yield return txResponse;
                }
            }
            
            
            queryWindow.Advance(response.Pagination);

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
    
    
    private class QueryWindow
    {
        public int WindowBlockWidth { get; set; }
        public int StartBlockHeight { get; set; }
        public int EndBlockHeight => StartBlockHeight + WindowBlockWidth;
        public int PaginationLimit { get; set; }
        public int PaginationOffset { get; set; }

        public string[] GetConditions()
        {
            return new[]
            {
                $"tx.height>={StartBlockHeight}",
                $"tx.height<={EndBlockHeight}"
            };
        }

        public void Advance(LcdPagination pagination)
        {
            // did we reach the final page of this "block window"?
            if (IsFinalPage(pagination))
            {
                // advance to the next block window
                StartBlockHeight += WindowBlockWidth;
                PaginationOffset = 0;
            }
            else
            {
                // advance to the next page of transactions
                // within the current block window
                PaginationOffset += PaginationLimit;
            }
        }

        private bool IsFinalPage(LcdPagination pagination)
        {
            return pagination.TotalAsInt <= (PaginationOffset + PaginationLimit);
        }
    }
}