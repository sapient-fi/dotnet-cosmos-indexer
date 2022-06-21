namespace TerraDotnet.TerraLcd.Messages;

public record LcdTxResponseEventAttribute
{
    public string Key { get; set; }
    public string Value { get; set; }
    public bool? Index { get; set; }
}