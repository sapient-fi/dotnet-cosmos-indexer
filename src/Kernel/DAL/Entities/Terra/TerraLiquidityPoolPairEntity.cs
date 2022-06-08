using ServiceStack.DataAnnotations;

namespace Sapient.Kernel.DAL.Entities.Terra;

public class TerraLiquidityPoolPairEntity
{
    [PrimaryKey] public long Id { get; set; }
        
    [ForeignKey(typeof(TerraRawTransactionEntity))]
    public long TransactionId { get; set; }

    public string AssetOneDenominator { get; set; }

    public decimal AssetOneQuantity { get; set; }

    public decimal? AssetOneUstValue { get; set; }

    public string AssetTwoDenominator { get; set; }

    public decimal AssetTwoQuantity { get; set; }

    public decimal? AssetTwoUstValue { get; set; }

    public decimal AssetLpQuantity { get; set; }
        
    [Index]
    public string Farm { get; set; }
    
    [Index]
    [Required]
    public DecentralizedExchange Dex { get; set; }
        
    public DateTimeOffset CreatedAt { get; set; }
    
    [Index]
    public string Wallet { get; set; }
}