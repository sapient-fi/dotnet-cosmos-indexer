using MassTransit;
using Microsoft.Extensions.Logging;

namespace SapientFi.Infrastructure.Terra2.TransactionListener;

/// <summary>
/// "Listens" to transactions from the Terra 2 blockchain,
/// then stores the raw transactions and announces to the
/// rest of the system that a transaction is ready for
/// processing by interested parties.
/// </summary>
public class Terra2TransactionListener
{
    private readonly ILogger<Terra2TransactionListener> _logger;
    private readonly IBus _mtBus;

    public Terra2TransactionListener(ILogger<Terra2TransactionListener> logger, IBus mtBus)
    {
        _logger = logger;
        _mtBus = mtBus;
    }
    
    
}