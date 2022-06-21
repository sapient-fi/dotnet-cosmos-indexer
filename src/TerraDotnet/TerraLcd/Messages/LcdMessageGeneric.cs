using System.Text.Json;

namespace TerraDotnet.TerraLcd.Messages;

public record LcdMessageGeneric
{
    /// <summary>
    /// The type of the transaction - contains valuable information on how the parse the <see cref="Value"/>
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Generic blobbed storage to carry arbitrary data
    /// </summary>
    public Dictionary<string, JsonElement> Value { get; set; } = new();
}