using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages
{
    public record TxLogEvent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("attributes")]
        public List<TxLogEventAttribute> Attributes { get; set; }
    }
}