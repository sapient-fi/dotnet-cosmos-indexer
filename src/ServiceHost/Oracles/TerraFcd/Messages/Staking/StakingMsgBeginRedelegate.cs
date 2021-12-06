using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Staking
{
    public record StakingMsgBeginRedelegate : IMsg
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}