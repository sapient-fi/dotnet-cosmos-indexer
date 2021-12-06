using System.Diagnostics.CodeAnalysis;
using ServiceStack.DataAnnotations;

namespace Pylonboard.ServiceHost.DAL.TerraMoney;

public class TerraMineStakingEntity
{
    [PrimaryKey]
    public long Id { get; set; }
        
    [ForeignKey(typeof(TerraRawTransactionEntity))]
    public long TransactionId { get; set; }
        
    public string Sender { get; set; }

    public decimal Amount { get; set; }
        
    [NotNull]
    public DateTimeOffset CreatedAt { get; set; }

    public string TxHash { get; set; }
        
    [Index]
    [NotNull]
    public bool IsBuyBack { get; set; }
}