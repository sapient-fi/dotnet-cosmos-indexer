namespace SapientFi.Infrastructure.Oracles.ExchangeRates.Terra.LowLevel;

public record TerraMoneyExchangeRateResponse 
{
    public long Time { get; set; }

    public decimal Open { get; set; }

    public decimal High { get; set; }

    public decimal Low { get; set; }

    public decimal Close { get; set; }

    public decimal Volume { get; set; }
}