namespace Sapient.ServiceHost.Endpoints.MineRankings;

public record MineRankingPercentileData
{
    public decimal PercentileFloor { get; set; }

    public int WalletsIncluded { get; set; }

    public decimal AmountOfMine { get; set; }
}