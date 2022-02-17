using Refit;

namespace TerraDotnet.TerraFcd;

public interface ITerraMoneyFcdApiClient
{
    // ?offset=0&limit=100&account=terra14qul6swv2p3vcfqk38fm8dvkezf0gj52m6a78k
    [Get("/v1/txs")]
    public Task<TerraTxesResponse> ListTxesAsync(long offset, int limit, string account);
        
}