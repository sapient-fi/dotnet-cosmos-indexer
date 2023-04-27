using System;
using SapientFi.Infrastructure.Cosmos.Storage;
using ServiceStack.DataAnnotations;

namespace SapientFi.Infrastructure.Kujira.Storage;

public class KujiraRawTransactionEntity : ICosmosRawTransactionEntity
{
    [PrimaryKey]
    public long Id { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    
    [Unique]
    public string TxHash { get; init; } = string.Empty;
    
    [PgSqlJsonB]
    public string RawTx { get; init; } = string.Empty;
    
    [Index]
    public int Height { get; init; }
}
