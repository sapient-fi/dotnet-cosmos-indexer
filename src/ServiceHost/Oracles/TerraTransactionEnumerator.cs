using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using NewRelic.Api.Agent;
using Polly;
using Pylonboard.ServiceHost.Oracles.TerraFcd;
using Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;
using Refit;

namespace Pylonboard.ServiceHost.Oracles;

public class TerraTransactionEnumerator
{
    private readonly ILogger<TerraTransactionEnumerator> _logger;
    private readonly ITerraMoneyFcdApiClient _terraClient;

    public TerraTransactionEnumerator(
        ILogger<TerraTransactionEnumerator> logger,
        ITerraMoneyFcdApiClient terraClient
    )
    {
        _logger = logger;
        _terraClient = terraClient;
    }

    [Trace]
    public async IAsyncEnumerable<(TerraTxWrapper tx, CoreStdTx msg)> EnumerateTransactionsAsync(
        long offset,
        int limit,
        string contract,
        [EnumeratorCancellation] CancellationToken stoppingToken
    )
    {
        var response = await Policy
            .Handle<ApiException>(exception => exception.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(10, retryCounter => TimeSpan.FromMilliseconds(Math.Pow(10, retryCounter)),
                (exception, span) =>
                {
                    _logger.LogWarning("Handling retry while enumerating Terra Transactions, waiting {Time:c}", span);
                })
            .ExecuteAsync(async () => await _terraClient.ListTxesAsync(offset, limit, contract));
        var txes = response;
        var next = txes.Next;
        var stopwatch = new Stopwatch();
        var doContinue = true;
        do
        {
            if (next == 0)
            {
                doContinue = false;
            }
            
            _logger.LogInformation("`next` continuation token: {Next}", next);

            // NO await to perform background work
            // Capture the async method as a func so we can invoke it again during retry
            Func<Task<TerraTxesResponse>> listTxesAsync = () => _terraClient.ListTxesAsync(
                txes.Next,
                txes.Limit,
                contract
            );
            var nextTxes = listTxesAsync();
            stopwatch.Reset();
            stopwatch.Start();
            foreach (var tx in txes.Txs)
            {
                if (tx.Code != 0)
                {
                    _logger.LogInformation("Skipping tx with Id {Id} since code is {Code}", tx.Id, tx.Code);
                    continue;
                }

                _logger.LogDebug("Processing transaction with id {Id}", tx.Id);

                var msg = TerraTransactionValueFactory.GetIt(tx);
                yield return (tx, msg);
            }
            stopwatch.Stop();

            var expectedWallTime = 2d;
            if (stopwatch.Elapsed.TotalSeconds < expectedWallTime)
            {
                var nappieTime = TimeSpan.FromSeconds(expectedWallTime-stopwatch.Elapsed.TotalSeconds);
                _logger.LogDebug("Rate limiting, less than 2 seconds was spent on processing by the consumer. Napping for {Sleep}", nappieTime.ToString("g"));
                await Task.Delay(nappieTime, stoppingToken);
            }
            _logger.LogDebug("waiting for next http result-set");

            Policy.Handle<ApiException>(exception => exception.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetry(10, retryCounter => TimeSpan.FromMilliseconds(Math.Pow(10, retryCounter)),
                    (exception, span) =>
                    {
                        _logger.LogWarning("Handling retry while enumerating Terra Transactions, waiting {Time:c}", span);
                        nextTxes = listTxesAsync();
                    })
                .Execute(() => { Task.WaitAll(new Task[] { nextTxes }, cancellationToken: stoppingToken); });

            _logger.LogDebug("done, iterating");
            txes = nextTxes.Result;
            next = nextTxes.Result.Next;
        } while (doContinue);
    }
}