using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record LcdTx
{
    [JsonPropertyName("body")]
    public LcdTxBody Body { get; set; } = new();
    
    // TODO auth_info
    
    // TODO signatures
}