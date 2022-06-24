using SapientFi.Infrastructure.Terra2.Storage;
using SapientFi.Kernel.IdGeneration;
using TerraDotnet;
using TerraDotnet.TerraLcd.Messages;
using System.Text.Json;

namespace SapientFi.Infrastructure.Terra2;

public class Terra2Factory
{
    private readonly IdProvider _idProvider;

    public Terra2Factory(IdProvider idProvider)
    {
        _idProvider = idProvider;
    }

    public virtual Terra2RawTransactionEntity NewRawEntity(LcdTxResponse lcdTransaction)
    {
        return new()
        {
            Id = _idProvider.Snowflake(),
            Height = lcdTransaction.HeightAsInt,
            CreatedAt = lcdTransaction.CreatedAt,
            TxHash = lcdTransaction.TransactionHash,
            RawTx = JsonSerializer.Serialize(lcdTransaction, TerraJsonSerializerOptions.GetThem())
        };
    }
}