namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmMsgInstantiateContract : IMsg
{
    public string Type { get; set; } = string.Empty;
}