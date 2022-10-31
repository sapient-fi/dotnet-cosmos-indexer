using Microsoft.Extensions.Configuration;
using RapidCore.Configuration;
using SapientFi.Infrastructure.Cosmos.TransactionListener;

namespace SapientFi.Infrastructure.Terra2.TransactionListener;

public class Terra2TransactionListenerConfig : CosmosTransactionListenerConfig
{
    public Terra2TransactionListenerConfig(IConfiguration raw) : base(raw)
    {
    }

    public override bool DoEnable() => Raw.Get("Terra2_TransactionListener_DoEnable", true);
}