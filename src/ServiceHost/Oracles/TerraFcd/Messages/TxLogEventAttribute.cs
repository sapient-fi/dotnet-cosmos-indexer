using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages
{
    public record TxLogEventAttribute
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }
        
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
}