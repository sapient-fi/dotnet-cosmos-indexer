using Microsoft.Extensions.Configuration;
using RapidCore.Configuration;
using SapientFi.Infrastructure.Cosmos.TransactionListener;

namespace SapientFi.Infrastructure.Kujira.TransactionListener;

public class KujiraTransactionListenerConfig : CosmosTransactionListenerConfig
{
    public KujiraTransactionListenerConfig(IConfiguration raw) : base(raw)
    {
    }

    public override bool DoEnable() => Raw.Get("KUJIRA__TRANSACTION_LISTENER__DO_ENABLE", true);

    public override string LcdUri() =>
        Raw.Get("KUJIRA__TRANSACTION_LISTENER__LCD_URI", "https://lcd.kaiyo.kujira.setten.io");
}
