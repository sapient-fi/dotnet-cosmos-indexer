namespace Pylonboard.Kernel;

public record TerraAmount
{
    public decimal Value { get; set; }
        
    public string Denominator { get; set; }
}