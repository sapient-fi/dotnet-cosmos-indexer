namespace Sapient.ServiceHost.Endpoints.MineTreasury;

public record MineTreasuryGraph
{
    public IEnumerable<MineBuybackGraph> Buybacks { get; set; }
}