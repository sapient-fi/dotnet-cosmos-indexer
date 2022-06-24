using System.IO;
using System.Text.Json;
using MassTransit;
using SapientFi.Infrastructure.Terra2.BusMessages;
using TerraDotnet;
using TerraDotnet.TerraLcd.Messages;

namespace UnitTests.TerraDotnet.TerraLcd.Transactions;

public class RawTransactionIndexerTests
{
    private readonly IBus _massTransitBus;

    public RawTransactionIndexerTests(
        IBus massTransitBus
    )
    {
        _massTransitBus = massTransitBus;
    }

    private GetTransactionsMatchingQueryResponse? GetParsedJson()
    {
        var json = File.ReadAllText("TerraDotnet/TerraLcd/Messages/RawTransaction_D670_01.json");

        return JsonSerializer.Deserialize<GetTransactionsMatchingQueryResponse>(
            json,
            TerraJsonSerializerOptions.GetThem()
        );
    }
    
    
    
}