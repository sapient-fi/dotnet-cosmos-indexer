using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd.Messages;

namespace TerraDotnet.TerraFcd;

public static class TerraTransactionValueFactory
{
    public static CoreStdTx GetIt(TerraTxWrapper tx)
    {
        return tx.Tx.Type switch
        {
            "core/StdTx" => new CoreStdTx{
                Id = tx.Id,
                TransactionHash = tx.TransactionHash,
                Fee = tx.Tx.Value.ToObject<TxFee>("fee"),
                Memo = tx.Tx.Value.ToObject<string>("memo"),
                Messages = tx.Tx.Value.FromCoreStdTxMessage("msg", tx.ChainId, tx.TransactionHash),
                Logs = tx.Logs,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(tx.Tx.Type), $"Unable to handle tx type: {tx.Tx.Type}"),
        };
    }
}