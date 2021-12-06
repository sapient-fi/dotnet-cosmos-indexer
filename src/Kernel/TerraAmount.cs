namespace Pylonboard.Kernel;

public record Amount
{
    public decimal Value { get; set; }
        
    public string Denominator { get; set; }
}