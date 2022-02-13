using System.Diagnostics.CodeAnalysis;
using ServiceStack.DataAnnotations;

namespace Pylonboard.ServiceHost.DAL.TerraMoney;

public class TerraRawTransactionEntity
{
    [PrimaryKey]
    public long Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
        
    [Unique]
    public string TxHash { get; set; }

    [PgSqlJsonB]
    public string RawTx { get; set; }
}