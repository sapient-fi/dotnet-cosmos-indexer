namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmMsgExecuteContract : IMsg
{
    public string Type { get; set; }
    public IWasmMsgExecuteContractValue Value { get; set; }
}