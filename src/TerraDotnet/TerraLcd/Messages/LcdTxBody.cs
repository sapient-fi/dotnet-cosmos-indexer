using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record LcdTxBody
{
    public List<LcdMessageGeneric> Messages { get; set; } = new();

    public string Memo { get; set; } = string.Empty;

    [JsonPropertyName("timeout_height")]
    public string TimeoutHeight { get; set; } = string.Empty;
    
    // TODO extension_options
    
    // TODO non_critical_extension_options
}