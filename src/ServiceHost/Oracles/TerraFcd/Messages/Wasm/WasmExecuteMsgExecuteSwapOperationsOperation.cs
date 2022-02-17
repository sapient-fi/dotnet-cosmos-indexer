using System.Text.Json.Serialization;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgExecuteSwapOperationsOperation
{
    [JsonPropertyName("terra_swap")]
    public WasmExecuteMsgExecuteSwapOperationsOperationTerraSwap? TerraSwap { get; set; }
    
    [JsonPropertyName("native_swap")]
    public WasmExecuteMsgExecuteSwapOperationsOperationNativeSwap? NativeSwap { get; set; }
}