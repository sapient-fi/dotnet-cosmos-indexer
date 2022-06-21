namespace TerraDotnet.TerraLcd.Messages;

public record LcdTx
{
    public LcdTxBody Body { get; set; } = new();
    
    // TODO auth_info
    
    // TODO signatures
}