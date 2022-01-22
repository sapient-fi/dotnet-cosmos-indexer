namespace Pylonboard.ServiceHost.Config;

public interface IFeatureConfig
{
    public bool TriggerFullResync { get; }
}