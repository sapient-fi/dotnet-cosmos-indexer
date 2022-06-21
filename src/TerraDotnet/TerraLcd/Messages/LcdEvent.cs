namespace TerraDotnet.TerraLcd.Messages;

public record LcdEvent
{
    public string Type { get; set; } = string.Empty;

    public List<LcdTxResponseEventAttribute> Attributes { get; set; } = new();
}