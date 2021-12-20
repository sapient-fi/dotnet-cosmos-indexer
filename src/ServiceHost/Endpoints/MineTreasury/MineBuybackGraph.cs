namespace Pylonboard.ServiceHost.Endpoints.MineTreasury;

public record MineBuybackGraph
{
    public string TransactionHash { get; set; }
    
    public decimal MineAmount { get; set; }

    public decimal UstAmount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}