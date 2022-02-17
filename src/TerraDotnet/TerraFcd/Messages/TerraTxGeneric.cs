using System.Text.Json;

namespace TerraDotnet.TerraFcd.Messages;

public class TerraTxGeneric
{
    /// <summary>
    /// The type of the transaction - contains valuable information on how the parse the <see cref="Value"/>
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Generic blobbed storage to carry arbitrary 
    /// </summary>
    public Dictionary<string, JsonElement> Value { get; set; }
}