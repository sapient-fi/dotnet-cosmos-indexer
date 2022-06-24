using MassTransit.Futures.Contracts;

namespace SapientFi.ServiceHost.Endpoints.Staking;

public class ValidatorStakingGraph
{
    public List<ValidatorStakingItemGraph> ValidatorStakings { get; set; } = new();
}

public class ValidatorStakingItemGraph
{
    public List<(string, long)> ValidatorStakingList { get; set; } = new();
    public DateTime? From { get; set; } = null;
    public DateTime? To { get; set; } = null;
}
