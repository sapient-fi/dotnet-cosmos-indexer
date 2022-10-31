using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NewRelic.Api.Agent;
using Polly;
using Refit;
using TerraDotnet.TerraLcd;
using TerraDotnet.TerraLcd.Messages;

// ReSharper disable MemberCanBePrivate.Global

namespace TerraDotnet;

public struct EnumeratorOptions
{
    public EnumeratorOptions(int secondsPerBlock)
    {
        SecondsPerBlock = secondsPerBlock;
    }

    public int SecondsPerBlock { get; init; }
    public int WindowBlockWidth { get; init; } = 2000;
    public int PaginationLimit { get; init; } = 200;
}

public abstract class CosmosTransactionEnumerator<TMarker>
{
    protected readonly ILogger<CosmosTransactionEnumerator<TMarker>> Logger;
    protected readonly ICosmosLcdApiClient<TMarker> CosmosClient;
    protected int SecondsPerBlock { get; }
    protected int WindowBlockWidth { get; }
    protected int PaginationLimit { get; }
    
    protected CosmosTransactionEnumerator(
        ILogger<CosmosTransactionEnumerator<TMarker>> logger,
        ICosmosLcdApiClient<TMarker> cosmosClient,
        EnumeratorOptions options
    )
    {
        Logger = logger;
        CosmosClient = cosmosClient;
        SecondsPerBlock = options.SecondsPerBlock;
        WindowBlockWidth = options.WindowBlockWidth;
        PaginationLimit = options.PaginationLimit;
    }

    [Trace]
    public async IAsyncEnumerable<LcdTxResponse> EnumerateTransactionsAsync(
        int fromAndIncludingBlockHeight,
        [EnumeratorCancellation] CancellationToken stoppingToken
    )
    {
        var queryWindow = new QueryWindow
        {
            WindowBlockWidth = WindowBlockWidth,
            StartBlockHeight = fromAndIncludingBlockHeight,
            PaginationLimit = PaginationLimit,
            PaginationOffset = 0
        };
        
        // Fetch the latest block in the chain before we start enumerating. Will be used later.
        var latestBlockResponse = await CosmosClient.GetLatestBlockAsync();
        if (!latestBlockResponse.IsSuccessStatusCode)
        {
            Logger.LogCritical(latestBlockResponse.Error, "Unable to request latest block, abort");
            throw new Exception("latest block request went wrong... figure it out", latestBlockResponse.Error);
        }
        int latestBlockHeight = latestBlockResponse.Content.Block.Header.HeightAsInt;

        while (true)
        {
            var response = await Policy
                .Handle<ApiException>(exception => exception.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(10, retryCounter => TimeSpan.FromMilliseconds(Math.Pow(10, retryCounter)),
                    (_, span) =>
                    {
                        Logger.LogWarning("Handling retry while enumerating Cosmos Transactions, waiting {Time:c}", span);
                    }
                )
                .ExecuteAsync(
                    async () => await CosmosClient.GetTransactionsMatchingQueryAsync(
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
                    await Task.Delay(TimeSpan.FromSeconds(SecondsPerBlock), stoppingToken);
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