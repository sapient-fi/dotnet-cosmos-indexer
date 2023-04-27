using System.Text.Json.Serialization;

namespace TerraDotnet.TerraLcd.Messages;

public record LcdPagination
{
    [JsonPropertyName("next_key")]
    public string? NextKey { get; set; }

    public string Total { get; set; } = string.Empty;

    public int TotalAsInt => int.Parse(Total);
}