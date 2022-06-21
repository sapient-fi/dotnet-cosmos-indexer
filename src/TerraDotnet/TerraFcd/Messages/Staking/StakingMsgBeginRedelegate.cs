using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Staking;

public record StakingMsgBeginRedelegate : IMsg
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}