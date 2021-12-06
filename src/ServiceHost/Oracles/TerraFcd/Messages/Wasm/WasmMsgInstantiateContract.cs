namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public record WasmMsgInstantiateContract : IMsg
    {
        public string Type { get; set; }
    }
}