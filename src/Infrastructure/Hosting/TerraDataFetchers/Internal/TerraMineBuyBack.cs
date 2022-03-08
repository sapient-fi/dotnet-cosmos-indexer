namespace Pylonboard.Infrastructure.Hosting.TerraDataFetchers.Internal;

public record TerraMineBuyBack
{
    public long TxId { get; set; }
    public string TxHash { get; set; }
    public decimal UstAmount { get; set; }
    public decimal AmountInU { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}