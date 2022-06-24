using System.Text.Json.Serialization;

namespace TerraDotnet;

public record TerraAmount
{
    public TerraAmount(string amount, string denomOrToken)
    {
        var divisor = TerraDenominators.GetDenomOrTokenDivisor(denomOrToken);
        Value = long.Parse(amount) / divisor;
        Denominator = denomOrToken;
        Divisor = divisor;
    }

    public decimal Divisor { get; }

    [JsonPropertyName("amount")]
    public decimal Value { get; }

    [JsonPropertyName("denom")]
    public string Denominator { get; set; }
}