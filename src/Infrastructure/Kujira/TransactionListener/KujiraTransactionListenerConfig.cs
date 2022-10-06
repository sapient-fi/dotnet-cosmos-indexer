using Microsoft.Extensions.Configuration;
using RapidCore.Configuration;

namespace SapientFi.Infrastructure.Kujira.TransactionListener;

public class KujiraTransactionListenerConfig
{
    private readonly IConfiguration _raw;

    public KujiraTransactionListenerConfig(IConfiguration raw)
    {
        _raw = raw;
    }

    public virtual bool DoEnable => _raw.Get("Terra2:TransactionListener:DoEnable", false);
}