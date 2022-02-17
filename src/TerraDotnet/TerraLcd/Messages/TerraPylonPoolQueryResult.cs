using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record TerraPylonPoolQueryResult
{
    /// <summary>
    /// The amount of tokens accumulated in U notation
    /// </summary>
    [JsonPropertyName("amount")]
    public string Amount { get; set; }
}