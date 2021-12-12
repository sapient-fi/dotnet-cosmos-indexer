using Pylonboard.ServiceHost.Endpoints.Types;

namespace Pylonboard.ServiceHost.Endpoints.MineStakingStats;

public class MineStakingStatsGraph
{
    public IEnumerable<TimeSeriesStatEntry> StakedPerDay { get; set; }
    public IEnumerable<TimeSeriesStatEntry> StakedPerDayCumulative { get; set; }
}