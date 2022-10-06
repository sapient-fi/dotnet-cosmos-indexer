using TerraDotnet.TerraFcd.Messages;

namespace TerraDotnet.TerraLcd.Messages;

public record CosmosDelegateMessage : IMsg
{
    public string Type { get; set; } = string.Empty;

    public string DelegatorAddress { get; set; } = string.Empty;

    public string ValidatorAddress { get; set; } = string.Empty;

    public CosmosAmount? Amount { get; set; }
}