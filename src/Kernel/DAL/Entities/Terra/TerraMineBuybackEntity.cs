namespace Sapient.Kernel.DAL.Entities.Terra;

public record TerraMineBuybackEntity
{
    public long Id { get; set; }

    public long TransactionId { get; set; }

    public string TransactionHash { get; set; }

    public decimal MineAmount { get; set; }
    
    public decimal UstAmount { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
}