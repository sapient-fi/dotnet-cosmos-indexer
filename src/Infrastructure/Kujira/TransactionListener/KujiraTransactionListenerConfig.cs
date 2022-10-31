using Microsoft.Extensions.Configuration;
using RapidCore.Configuration;
using SapientFi.Infrastructure.Cosmos.TransactionListener;

namespace SapientFi.Infrastructure.Kujira.TransactionListener;

public class KujiraTransactionListenerConfig : CosmosTransactionListenerConfig
{
    public KujiraTransactionListenerConfig(IConfiguration raw) : base(raw)
    {
    }

    public override bool DoEnable() => Raw.Get("Kujira_TransactionListener_DoEnable", true);
}
