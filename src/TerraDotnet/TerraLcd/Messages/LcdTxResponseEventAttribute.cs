namespace TerraDotnet.TerraLcd.Messages;

public record LcdTxResponseEventAttribute
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public bool? Index { get; set; }
}