using System;

namespace SapientFi.Infrastructure.Cosmos.Storage;

public interface ICosmosRawTransactionEntity
{
    public long Id { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
        
    public string TxHash { get; init; }

    public string RawTx { get; init; }
    
    public int Height { get; init; }
}
