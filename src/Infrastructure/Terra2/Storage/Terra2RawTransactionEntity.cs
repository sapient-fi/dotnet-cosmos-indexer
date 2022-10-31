using System;
using ServiceStack.DataAnnotations;

namespace SapientFi.Infrastructure.Terra2.Storage;

public class Terra2RawTransactionEntity
{
    [PrimaryKey]
    public long Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
        
    [Unique]
    public string TxHash { get; set; } = string.Empty;

    [PgSqlJsonB]
    public string RawTx { get; set; } = string.Empty;
    
    [Index]
    public int Height { get; set; }
}