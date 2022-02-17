using ServiceStack.DataAnnotations;

namespace Pylonboard.Kernel.DAL.Entities.Terra;

public record TerraLiquidityPoolEntity
{
    [PrimaryKey]
    public long Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    
    [Index]
    public long TransactionId { get; set; }

    [Index]
    public string ContractAddr { get; set; }
    
    [Index]
    public string SenderAddr { get; set; }

    public string OfferAsset { get; set; }

    public decimal OfferAmount { get; set; }

    public string AskAsset { get; set; }

    public decimal AskAmount { get; set; }
}