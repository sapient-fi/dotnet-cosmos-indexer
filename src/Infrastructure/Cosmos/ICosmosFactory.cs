using SapientFi.Infrastructure.Cosmos.Storage;
using TerraDotnet.TerraLcd.Messages;

namespace SapientFi.Infrastructure.Cosmos;

public interface ICosmosFactory<out TRawTransactionEntity>
    where TRawTransactionEntity : ICosmosRawTransactionEntity, new()
{
    public TRawTransactionEntity NewRawEntity(LcdTxResponse lcdTransaction);
}
