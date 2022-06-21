using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public interface ILcdMessage
{
    [JsonPropertyName("@type")]
    public string Type { get; set; }
}