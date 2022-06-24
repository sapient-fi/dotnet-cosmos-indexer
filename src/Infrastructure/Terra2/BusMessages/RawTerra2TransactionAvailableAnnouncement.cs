namespace SapientFi.Infrastructure.Terra2.BusMessages;

public class RawTerra2TransactionAvailableAnnouncement
{
    public string TransactionHash { get; set; } = string.Empty;
    
    public long RawEntityId { get; set; }
}