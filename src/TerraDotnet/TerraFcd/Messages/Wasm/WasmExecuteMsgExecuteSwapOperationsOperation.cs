using System.Text.Json.Serialization;

namespace TerraDotnet.TerraFcd.Messages.Wasm;

public record WasmExecuteMsgExecuteSwapOperationsOperation
{
    [JsonPropertyName("terra_swap")]
    public WasmExecuteMsgExecuteSwapOperationsOperationTerraSwap? TerraSwap { get; set; }
    
    [JsonPropertyName("native_swap")]
    public WasmExecuteMsgExecuteSwapOperationsOperationNativeSwap? NativeSwap { get; set; }
}