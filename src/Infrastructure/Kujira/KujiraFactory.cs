using System.Text.Json;
using SapientFi.Infrastructure.Kujira.Storage;
using SapientFi.Kernel.IdGeneration;
using TerraDotnet;
using TerraDotnet.TerraLcd.Messages;

namespace SapientFi.Infrastructure.Kujira;

public class KujiraFactory
{
    private readonly IdProvider _idProvider;

    public KujiraFactory(IdProvider idProvider)
    {
        _idProvider = idProvider;
    }

    public virtual KujiraRawTransactionEntity NewRawEntity(LcdTxResponse lcdTransaction)
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