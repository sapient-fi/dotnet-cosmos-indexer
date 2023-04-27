using MassTransit;

namespace SapientFi.Infrastructure.Indexing;

public interface IIndexer<TAnnouncement> : IConsumer<TAnnouncement> where TAnnouncement : class
{
    string Id { get; }
    
    string DisplayName { get; }
}