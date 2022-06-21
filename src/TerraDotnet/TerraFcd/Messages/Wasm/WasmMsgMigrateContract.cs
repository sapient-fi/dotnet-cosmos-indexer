namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmMsgMigrateContract : IMsg
{
    public string Type { get; set; } = string.Empty;
}