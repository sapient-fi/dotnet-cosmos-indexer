using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages
{
    public record TxLog
    {
        [JsonPropertyName("events")]
        public List<TxLogEvent> Events { get; set; }
    }
}