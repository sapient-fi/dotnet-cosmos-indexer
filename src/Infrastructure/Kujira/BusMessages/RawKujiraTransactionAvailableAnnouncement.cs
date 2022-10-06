namespace SapientFi.Infrastructure.Kujira.BusMessages;

public class RawKujiraTransactionAvailableAnnouncement
{
    public string TransactionHash { get; set; } = string.Empty;
    
    public long RawEntityId { get; set; }
}