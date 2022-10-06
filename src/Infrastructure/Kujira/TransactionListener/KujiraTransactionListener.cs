using MassTransit;
using Microsoft.Extensions.Logging;

namespace SapientFi.Infrastructure.Kujira.TransactionListener;

/// <summary>
/// "Listens" to transactions from the Terra 2 blockchain,
/// then stores the raw transactions and announces to the
/// rest of the system that a transaction is ready for
/// processing by interested parties.
/// </summary>
public class KujiraTransactionListener
{
    private readonly ILogger<KujiraTransactionListener> _logger;
    private readonly IBus _mtBus;

    public KujiraTransactionListener(ILogger<KujiraTransactionListener> logger, IBus mtBus)
    {
        _logger = logger;
        _mtBus = mtBus;
    }
    
    
}