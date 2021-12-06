namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmMsgMigrateContract : IMsg
{
    public string Type { get; set; }
}