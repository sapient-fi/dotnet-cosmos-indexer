using System.Text.Json;
using SapientFi.Infrastructure.Cosmos.Storage;
using SapientFi.Kernel.IdGeneration;
using TerraDotnet;
using TerraDotnet.TerraLcd.Messages;

namespace SapientFi.Infrastructure.Cosmos;

public class CosmosFactory<TRawTransactionEntity>
    : ICosmosFactory<TRawTransactionEntity>
    where TRawTransactionEntity : ICosmosRawTransactionEntity, new()
{
    protected readonly IdProvider IdProvider;

    public CosmosFactory(IdProvider idProvider)
    {
        IdProvider = idProvider;
    }

    public virtual TRawTransactionEntity NewRawEntity(LcdTxResponse lcdTransaction)
    {
        return new()
        {
            Id = IdProvider.Snowflake(),
            Height = lcdTransaction.HeightAsInt,
            CreatedAt = lcdTransaction.CreatedAt,
            TxHash = lcdTransaction.TransactionHash,
            RawTx = JsonSerializer.Serialize(lcdTransaction, TerraJsonSerializerOptions.GetThem())
        };
    }
}
