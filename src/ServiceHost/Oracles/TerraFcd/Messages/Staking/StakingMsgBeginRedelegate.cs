using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Staking;

public record StakingMsgBeginRedelegate : IMsg
{
    [JsonPropertyName("type")]
    public string Type { get; set; }
}