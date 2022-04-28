using ServiceStack.DataAnnotations;

namespace Pylonboard.Kernel.DAL.Entities.Terra;

[CompositeIndex(
    nameof(Depositor),
    nameof(TransactionId),
    nameof(FriendlyName),
    nameof(CreatedAt),
    Name = "terra_pylon_pool_entity_dep_tx_fn_creat_uindex",
    Unique = true)]
public class TerraPylonPoolEntity
{
    public long Id { get; set; }

    [ForeignKey(typeof(TerraRawTransactionEntity))]
    public long TransactionId { get; set; }

    public string Depositor { get; set; }
        
    public string PoolContract { get; set; }

    public decimal Amount { get; set; }

    public string Denominator { get; set; }

    public TerraPylonPoolOperation Operation { get; set; }

    public TerraPylonPoolFriendlyName FriendlyName { get; set; }
        
    public DateTimeOffset CreatedAt { get; set; }
}

public enum TerraPylonPoolFriendlyName
{
    WhiteWhale1,
    WhiteWhale2,
    WhiteWhale3,
    Loop1,
    Loop2,
    Loop3,
    Orion,
    Valkyrie1,
    Valkyrie2,
    Valkyrie3,
    TerraWorld1,
    TerraWorld2,
    TerraWorld3,
    Mine1,
    Mine2,
    Mine3,
    Nexus,
    Glow1,
    Glow2,
    Glow3,
    Sayve1,
    Sayve2,
    Sayve3,
    Xdefi1,
    Xdefi2,
    Xdefi3,
    DeviantsFactions,
    GalaticPunks,
    LunaBulls
}

public enum TerraPylonPoolOperation
{
    /// <summary>
    /// Deposit funds to start accumulating rewards
    /// </summary>
    Deposit,
        
    /// <summary>
    /// Claim accumulated rewards
    /// </summary>
    Claim,
        
    /// <summary>
    /// Withdrawal of funds from a pool
    /// </summary>
    Withdraw,
    
    /// <summary>
    /// A user has bought a position on the market, i.e on bPSI-DP-24m
    /// </summary>
    Buy,
    
    
    /// <summary>
    /// A user has sold their position on the market, i.e. bPSI-DP-24m
    /// </summary>
    Sell
}