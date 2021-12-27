using ServiceStack.DataAnnotations;

namespace Pylonboard.ServiceHost.DAL.TerraMoney;

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
    GalaticPunks
}

public enum TerraPylonPoolOperation
{
    Deposit,
        
    Claim,
        
    Withdraw
}