using ServiceStack.DataAnnotations;

namespace Pylonboard.ServiceHost.DAL.TerraMoney;

public class TerraLpFarmEntity
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
        
    public DateTimeOffset CreatedAt { get; set; }
}