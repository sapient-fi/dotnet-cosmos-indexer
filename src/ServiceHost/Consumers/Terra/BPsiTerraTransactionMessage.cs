using Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;

namespace Pylonboard.ServiceHost.Consumers.Terra;

public record BPsiTerraTransactionMessage
{
    public long TransactionId { get; set; }
}