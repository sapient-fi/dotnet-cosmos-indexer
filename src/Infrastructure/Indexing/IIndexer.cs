namespace SapientFi.Infrastructure.Indexing;

public interface IIndexer
{
    string Id { get; }
    
    string DisplayName { get; }

    Task ProcessTransactionAsync(object tx);
}