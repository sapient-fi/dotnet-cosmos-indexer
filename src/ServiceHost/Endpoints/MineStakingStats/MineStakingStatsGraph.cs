using Sapient.ServiceHost.Endpoints.Types;

namespace Sapient.ServiceHost.Endpoints.MineStakingStats;

public class MineStakingStatsGraph
{
    public IEnumerable<TimeSeriesStatEntry> StakedPerDay { get; set; }
    public IEnumerable<TimeSeriesStatEntry> StakedPerDayCumulative { get; set; }
    public IEnumerable<DaysStakedStatEntry> DaysStakedBinned { get; set; }
    public IEnumerable<TimeSeriesStatEntry> NewWalletsPerDay { get; set; }
}