namespace Sapient.ServiceHost.Endpoints.Types;

public class TimeSeriesStatEntry
{
    public DateTimeOffset At { get; set; }
    public decimal Value { get; set; }
}