namespace Pylonboard.ServiceHost.Config;

public interface IMessageTransportConfig
{
    string TransportUri { get; }
}