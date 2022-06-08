namespace Sapient.ServiceHost.Endpoints.MineStakingStats;

public record DaysStakedStatEntry
{
    public int DaysStakedBin { get; set; }

    public decimal Count { get; set; }
}