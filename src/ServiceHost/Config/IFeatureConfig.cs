namespace Pylonboard.ServiceHost.Config;

public interface IFeatureConfig
{
    public bool TriggerGatewayPoolFullResync { get; }
    
    public bool TriggerMineStakingFullResync { get; }
    public bool TriggerMineBuyBackFullResync { get; }
}