namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmMsgInstantiateContract : IMsg
{
    public string Type { get; set; }
}