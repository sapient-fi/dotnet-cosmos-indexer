using System.Collections.Generic;
using Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd.Messages;

namespace Invacoil.ServiceRole.TerraMoney.Oracles.TerraFcd
{
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
}