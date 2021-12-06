using System.Text.Json.Serialization;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages.Wasm
{
    public record WasmExecuteMsgCastVote
    {
        [JsonPropertyName("vote")]
        public string Vote { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("poll_id")]
        public int PollId { get; set; }
    }
}