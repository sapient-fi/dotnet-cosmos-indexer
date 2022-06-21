using ServiceStack.DataAnnotations;
using TerraDotnet.TerraLcd.Messages;

namespace SapientFi.Infrastructure.Terra2.Storage;

public class Terra2RawTransactionEntity
{
    [PrimaryKey]
    public long Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
        
    [Unique]
    public string TxHash { get; set; } = string.Empty;

    [PgSqlJsonB]
    public LcdTxResponse RawTx { get; set; } = new();
    
    [Index]
    public int Height { get; set; }
}