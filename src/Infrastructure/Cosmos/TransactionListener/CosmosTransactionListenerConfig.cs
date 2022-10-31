using Microsoft.Extensions.Configuration;

namespace SapientFi.Infrastructure.Cosmos.TransactionListener;

public abstract class CosmosTransactionListenerConfig
{
    protected readonly IConfiguration Raw;

    protected CosmosTransactionListenerConfig(IConfiguration raw)
    {
        Raw = raw;
    }

    public abstract bool DoEnable();
}
