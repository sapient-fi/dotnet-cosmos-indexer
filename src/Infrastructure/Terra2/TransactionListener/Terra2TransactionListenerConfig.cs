using Microsoft.Extensions.Configuration;
using RapidCore.Configuration;

namespace SapientFi.Infrastructure.Terra2.TransactionListener;

public class Terra2TransactionListenerConfig
{
    private readonly IConfiguration _raw;

    public Terra2TransactionListenerConfig(IConfiguration raw)
    {
        _raw = raw;
    }

    public virtual bool DoEnable => _raw.Get("Terra2:TransactionListener:DoEnable", false);
}