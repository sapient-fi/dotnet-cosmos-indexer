using Microsoft.Extensions.Configuration;
using RapidCore.Configuration;
using SapientFi.Infrastructure.Cosmos.TransactionListener;

namespace SapientFi.Infrastructure.Terra2.TransactionListener;

public class Terra2TransactionListenerConfig : CosmosTransactionListenerConfig
{
    public Terra2TransactionListenerConfig(IConfiguration raw) : base(raw)
    {
    }

    public override bool DoEnable() => Raw.Get("TERRA2__TRANSACTION_LISTENER__DO_ENABLE", true);

    // Other options are:
    // "https://phoenix-lcd.sapient.fi"
    // "https://phoenix-lcd.terra.dev/"
    public override string LcdUri() =>
        Raw.Get("TERRA2__TRANSACTION_LISTENER__LCD_URI", "http://54.36.112.157:1317");
}