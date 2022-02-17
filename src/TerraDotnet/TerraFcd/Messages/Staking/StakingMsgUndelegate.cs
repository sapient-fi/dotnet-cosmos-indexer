using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Staking;

public record StakingMsgUndelegate : IMsg
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}