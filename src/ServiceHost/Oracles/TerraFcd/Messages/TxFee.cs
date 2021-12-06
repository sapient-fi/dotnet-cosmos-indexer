using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages
{
    public record TxFee
    {
        [JsonPropertyName("gas")]
        public string Gas { get; set; }
        [JsonPropertyName("amount")]
        public List<TerraStringAmount> Amount { get; set; }
    }
}