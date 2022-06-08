namespace Sapient.ServiceHost.Endpoints.Arbitraging;

public record ArbBands
{
    public DateTimeOffset CloseTime { get; set; }

    public decimal Close { get; set; }

    public decimal LowerBand { get; set; }

    public decimal UpperBand { get; set; }
}