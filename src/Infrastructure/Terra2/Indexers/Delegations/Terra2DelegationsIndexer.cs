using SapientFi.Infrastructure.Indexing;

namespace SapientFi.Infrastructure.Terra2.Indexers.Delegations;

public class Terra2DelegationsIndexer : IIndexer
{
    public string Id => "terra2_delegations";

    public string DisplayName => "Terra 2 Delegations";
    
    public Task ProcessTransactionAsync(object tx)
    {
        throw new NotImplementedException();
    }
}