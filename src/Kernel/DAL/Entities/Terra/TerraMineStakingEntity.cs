using ServiceStack.DataAnnotations;

namespace Sapient.Kernel.DAL.Entities.Terra;

public class TerraMineStakingEntity
{
    [PrimaryKey]
    public long Id { get; set; }
        
    [ForeignKey(typeof(TerraRawTransactionEntity))]
    public long TransactionId { get; set; }
        
    public string Sender { get; set; }

    public decimal Amount { get; set; }
        
    public DateTimeOffset CreatedAt { get; set; }

    public string TxHash { get; set; }
        
    [Index]
    public bool IsBuyBack { get; set; }
}