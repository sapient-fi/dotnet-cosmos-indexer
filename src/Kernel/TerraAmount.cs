namespace Pylonboard.Kernel;

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

    public decimal Value { get; }
        
    public string Denominator { get; set; }
}