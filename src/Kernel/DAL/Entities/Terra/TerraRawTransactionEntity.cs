using ServiceStack.DataAnnotations;

namespace SapientFi.Kernel.DAL.Entities.Terra;

public class TerraRawTransactionEntity
{
    [PrimaryKey]
    public long Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
        
    [Unique]
    public string TxHash { get; set; } = string.Empty;

    [PgSqlJsonB]
    public string RawTx { get; set; } = string.Empty;
}