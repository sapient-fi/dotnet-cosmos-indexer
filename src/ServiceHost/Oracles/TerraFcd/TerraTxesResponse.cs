using Pylonboard.ServiceHost.Oracles.TerraFcd.Messages;

namespace Pylonboard.ServiceHost.Oracles.TerraFcd;

public record TerraTxesResponse
{
    /// <summary>
    /// Used for paging, the value of Next should go in as Offset for the next call
    /// </summary>
    public long Next { get; set; }

    /// <summary>
    /// The limit applied to the returned data
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// The list of txes returned 
    /// </summary>
    public List<TerraTxWrapper> Txs { get; set; }
}