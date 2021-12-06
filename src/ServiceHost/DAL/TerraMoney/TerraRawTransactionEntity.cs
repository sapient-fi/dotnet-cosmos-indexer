using System;
using System.Diagnostics.CodeAnalysis;
using Invacoil.Kernel.DAL;
using ServiceStack.DataAnnotations;

namespace Invacoil.ServiceRole.TerraMoney.DAL
{
    public class TerraRawTransactionEntity
    {
        [PrimaryKey]
        public long Id { get; set; }
        
        [NotNull]
        
        public DateTimeOffset CreatedAt { get; set; }
        
        [Unique]
        public string TxHash { get; set; }

        [PgSqlJsonB]
        public string RawTx { get; set; }
    }
}