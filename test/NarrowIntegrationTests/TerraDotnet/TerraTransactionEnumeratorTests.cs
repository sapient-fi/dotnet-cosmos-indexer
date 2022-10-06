using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SapientFi.Infrastructure.Terra2;
using TerraDotnet;
using TerraDotnet.TerraLcd;
using Xunit;

namespace NarrowIntegrationTests.TerraDotnet;

public class TerraTransactionEnumeratorTests
{
    private readonly CosmosTransactionEnumerator<Terra2Marker> _transactionEnumerator;

    public TerraTransactionEnumeratorTests()
    {
        var services = new ServiceCollection();
        services.InternalAddLcdClient<Terra2Marker>("https://phoenix-lcd.terra.dev");

        var client = services.BuildServiceProvider().GetRequiredService<ICosmosLcdApiClient<Terra2Marker>>();
        
        _transactionEnumerator = new CosmosTransactionEnumerator<Terra2Marker>(
            NullLogger<CosmosTransactionEnumerator<Terra2Marker>>.Instance, 
            client
        );
    }

    [Fact(Skip = "this is only meant to be an easy way for devs to debug the enumerator")]
    public async Task TriggerTheEnumerator()
    {
        var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        
        var enumeration = _transactionEnumerator.EnumerateTransactionsAsync(0, tokenSource.Token);

        await foreach (var response in enumeration.WithCancellation(tokenSource.Token))
        {
            var stop = "hammer time";
        }
    }
}