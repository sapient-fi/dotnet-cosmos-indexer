namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public record WasmMsgMigrateContract : IMsg
    {
        public string Type { get; set; }
    }
}