using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record LcdTxResponseLog
{
    [JsonPropertyName("msg_index")]
    public int MessageIndex { get; set; }

    public string Log { get; set; } = string.Empty;

    public List<LcdEvent> Events { get; set; } = new();
    
    
}