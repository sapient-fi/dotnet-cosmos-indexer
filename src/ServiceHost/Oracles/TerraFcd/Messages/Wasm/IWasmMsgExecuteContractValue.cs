using System.Collections.Generic;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public interface IWasmMsgExecuteContractValue
    {
        List<Coin> Coins { get; set; }
        string Sender { get; set; }
        string Contract { get; set; }
        WasmExecuteMessage ExecuteMessage { get; set; }
    }
}