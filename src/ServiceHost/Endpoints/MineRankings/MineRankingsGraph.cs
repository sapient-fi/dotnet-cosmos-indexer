namespace Pylonboard.ServiceHost.Endpoints.MineRankings;

public record MineRankingsGraph
{
    public MineRankingPercentileData Percentile99 { get; set; }
    public MineRankingPercentileData Percentile95 { get; set; }
    public MineRankingPercentileData Percentile90 { get; set; }
    public MineRankingPercentileData Percentile75 { get; set; }
    public MineRankingPercentileData Floor { get; set; }
}