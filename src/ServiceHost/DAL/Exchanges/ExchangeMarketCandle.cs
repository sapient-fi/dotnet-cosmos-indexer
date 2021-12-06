using Pylonboard.Kernel;
using ServiceStack.DataAnnotations;

namespace Pylonboard.ServiceHost.DAL.Exchanges;

public class ExchangeMarketCandle
{
    [PrimaryKey]
    [AutoIncrement]
    public long Id { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public string Market { get; set; }
    public DateTimeOffset OpenTime { get; set; }
    public DateTimeOffset CloseTime { get; set; }
    public Exchange Exchange { get; set; }
}