namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmMsgExecuteContract : IMsg
{
    public string Type { get; set; } = string.Empty;
    public IWasmMsgExecuteContractValue? Value { get; set; }
}