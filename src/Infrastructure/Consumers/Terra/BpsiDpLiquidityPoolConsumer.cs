using System.Data;
using MassTransit;
using Microsoft.Extensions.Logging;
using Pylonboard.Infrastructure.Hosting.TerraDataFetchers;
using Pylonboard.Kernel.Contracts.Terra;
using Pylonboard.Kernel.DAL.Entities.Terra;
using Pylonboard.Kernel.IdGeneration;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TerraDotnet;
using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd;
using TerraDotnet.TerraFcd.Messages;
using TerraDotnet.TerraFcd.Messages.Wasm;

namespace Pylonboard.Infrastructure.Consumers.Terra;

public class BpsiDpLiquidityPoolConsumer : IConsumer<BPsiTerraTransactionMessage>
{
    private readonly ILogger<BpsiDpLiquidityPoolConsumer> _logger;
    private readonly IdGenerator _idGenerator;
    private readonly IDbConnectionFactory _dbFactory;

    public BpsiDpLiquidityPoolConsumer(
        ILogger<BpsiDpLiquidityPoolConsumer> logger,
        IDbConnectionFactory dbFactory,
        IdGenerator idGenerator
    )
    {
        _logger = logger;
        _idGenerator = idGenerator;
        _dbFactory = dbFactory;
    }

    public async Task Consume(ConsumeContext<BPsiTerraTransactionMessage> context)
    {
        using var db = _dbFactory.OpenDbConnection();
        var terraTxId = context.Message.TransactionId;
        var terraDbTx = await db.SingleByIdAsync<TerraRawTransactionEntity>(terraTxId);
        
        using var tx = db.OpenTransaction();
        var exists = await db.SingleAsync<long?>(db.From<TerraLiquidityPoolEntity>()
            .Where(q => q.TransactionId == terraTxId)
            .Take(1)
            .Select(e => 
            new
            {
                e.Id
            }));
        if (exists.HasValue)
        {
            return;
        }

        var terraTx = terraDbTx.RawTx.ToObject<TerraTxWrapper>();
        var msg = TerraTransactionValueFactory.GetIt(terraTx);
        var cancellationToken = context.CancellationToken;
        
        var results = ParseTransaction(msg, terraTx);

        if (!results.Any())
        {
            tx.Rollback();
            return;
        }
        
        foreach (var result in results)
        {
            await db.InsertAsync(result, token: cancellationToken);
            await HandleGatewayPoolInsertAsync(db, result, cancellationToken);
        }

        tx.Commit();
    }

    public List<TerraLiquidityPoolEntity> ParseTransaction(
        CoreStdTx msg, 
        TerraTxWrapper terraTx
    )
    {
        var results = new List<TerraLiquidityPoolEntity>();
        
        foreach (var properMsg in msg.Messages.Select(innerMsg => innerMsg as WasmMsgExecuteContract))
        {
            if (properMsg == default)
            {
                _logger.LogWarning("Transaction with id {Id} did not have a message of type {Type}", terraTx.Id,
                    typeof(WasmMsgExecuteContract));
                continue;
            }

            // Regular "send" message, meaning someone swapping bPSI -> PSI pr bPSI -> UST
            // This depends on the base64 encoded data in msg on the execute message
            var executeSend = properMsg.Value.ExecuteMessage?.Send;

            if (executeSend?.Contract != null)
            {
                // send example:
                // 1: swap operation bPSI -> PSI: https://finder.extraterrestrial.money/mainnet/tx/AB05DB9D0B57D6263C175CA91438CA6DE5394B1E3A09B9C7022E505BE843C5F1
                // 2. route swap: bPSI -> UST: https://finder.extraterrestrial.money/mainnet/tx/5942DB5E6AF447F0173A5CDAA0D3902026C85661E7FC54BF94CC496EC254ACD8

                // TODO stuff like this should be excluded : https://finder.extraterrestrial.money/mainnet/tx/469DCC4C1A3F4C9ADC82C45A532EB251020B202B26E81E1B2B49A17FCD5BA74E
                if (!executeSend.Contract.EqualsIgnoreCase(TerraLiquidityPoolContracts.BPSI_DP_CONTRACT)
                    && !executeSend.Contract.EqualsIgnoreCase(TerraLiquidityPoolContracts.TERRASWAP_ROUTE_SWAP)
                   )
                {
                    _logger.LogInformation("This was SEND on a different contract {Contract}",
                        executeSend.Contract);
                    continue;
                }

                var nestedMessageSwapOperations =
                    executeSend.Message.ToObjectFromBase64<WasmExecuteMessage>()?.ExecuteSwapOperations;

                if (nestedMessageSwapOperations?.Operations != null)
                {
                    var result = HandleSwapOperationsAsync(terraTx, properMsg, nestedMessageSwapOperations);
                    if (result != null) results.Add(result);
                    
                    continue;
                }

                var nestedSwap = executeSend.Message.ToObjectFromBase64<WasmExecuteMessage>()?.Swap;
                if (nestedSwap?.BeliefPrice != null || nestedSwap?.MaxSpread != null)
                {
                    var result = HandleSwapAsync(terraTx, properMsg, msg, properMsg);
                    if (result != null) results.Add(result);
                    continue;
                }
            }

            var routeSwap = properMsg.Value.ExecuteMessage?.ExecuteSwapOperations;
            if (routeSwap?.Operations != null)
            {
                var result = HandleSwapOperationsAsync(terraTx, properMsg, routeSwap);
                if (result != null) results.Add(result);
                continue;
            }

            var swap = properMsg.Value.ExecuteMessage?.Swap;
            if (swap?.BeliefPrice != null || swap?.MaxSpread != null)
            {
                var result = HandleSwapAsync(terraTx, properMsg, msg, properMsg);
                if (result != null) results.Add(result);

                continue;
            }

            _logger.LogWarning("Unhandled situation for LP handler on tx has: {TxHash}", terraTx.TransactionHash);
        }

        return results;
    }

    private TerraLiquidityPoolEntity? HandleSwapAsync(
        TerraTxWrapper tx,
        WasmMsgExecuteContract executeContract,
        CoreStdTx msg,
        WasmMsgExecuteContract properMsg
    )
    {
        var offerAsset = msg.Logs
            .QueryTxLogsForAttributeFirstOrDefault("from_contract", "offer_asset");
        var offerAmount = msg.Logs
            .QueryTxLogsForAttributeFirstOrDefault("from_contract", "offer_amount");
        var askAsset = msg.Logs
            .QueryTxLogsForAttributeFirstOrDefault("from_contract", "ask_asset");
        var returnAmount = msg.Logs
            .QueryTxLogsForAttributeFirstOrDefault("from_contract", "return_amount");

        if (!askAsset.Value.EqualsIgnoreCase(TerraTokenContracts.BPSI_DP_24M)
            && !offerAsset.Value.EqualsIgnoreCase(TerraTokenContracts.BPSI_DP_24M))
        {
            _logger.LogInformation("trade is not asking or offering bPSI, irrelevant");
            return default;
        }

        var entity = new TerraLiquidityPoolEntity
        {
            Id = _idGenerator.Snowflake(),
            AskAmount = new TerraAmount(returnAmount.Value, askAsset.Value).Value,
            AskAsset = askAsset.Value,
            ContractAddr = executeContract.Value.Contract,
            CreatedAt = tx.CreatedAt,
            OfferAmount = new TerraAmount(offerAmount.Value, offerAsset.Value).Value,
            OfferAsset = offerAsset.Value,
            SenderAddr = properMsg.Value.Sender,
            TransactionId = tx.Id
        };

        return entity;
    }

    private TerraLiquidityPoolEntity? HandleSwapOperationsAsync(
        TerraTxWrapper tx,
        WasmMsgExecuteContract executeContract,
        WasmExecuteMsgExecuteSwapOperations swapOperations
    )
    {
        var offerAsset = swapOperations.Operations.First(op => op.TerraSwap != default).TerraSwap?.OfferAssetInfo;
        var offerAssetContract = offerAsset.Token?.ContractAddress ?? offerAsset.NativeToken.Denominator;

        var offerAmountStr = swapOperations.OfferAmount
                             ?? executeContract.Value.ExecuteMessage?.Send?.Amount
                             ?? executeContract.Value.Coins.First().Amount;

        var offerAmount = new TerraAmount(offerAmountStr, offerAssetContract);
        var askAsset = swapOperations.Operations.Last(op => op.TerraSwap != default).TerraSwap?.AskAssetInfo;
        var askAssetContract = askAsset.Token?.ContractAddress ?? askAsset.NativeToken.Denominator;
        var returnAmountAttr = tx.Logs.QueryTxLogsForAttributeLastOrDefault("from_contract", "return_amount");
        var returnAmount = new TerraAmount(returnAmountAttr.Value, askAssetContract);

        if (!askAssetContract.EqualsIgnoreCase(TerraTokenContracts.BPSI_DP_24M)
            && !offerAssetContract.EqualsIgnoreCase(TerraTokenContracts.BPSI_DP_24M))
        {
            _logger.LogInformation("trade is not asking or offering bPSI, irrelevant");
            return default;
        }

        if (!tx.Logs.QueryTxLogsForAttributes("from_contract", attribute => attribute.Key.EqualsIgnoreCase("to")).Any(
                attr =>
                    attr.Value.EqualsIgnoreCase(TerraLiquidityPoolContracts.BPSI_DP_CONTRACT)))
        {
            throw new InvalidOperationException(
                $"Handling tx {tx.TransactionHash} and cannot find the bPSI DB contract in `to` on the logs");
        }

        var entity = new TerraLiquidityPoolEntity
        {
            Id = _idGenerator.Snowflake(),
            AskAmount = returnAmount.Value,
            AskAsset = askAssetContract,
            OfferAmount = offerAmount.Value,
            OfferAsset = offerAssetContract,
            CreatedAt = tx.CreatedAt,
            TransactionId = tx.Id,
            ContractAddr = TerraLiquidityPoolContracts.BPSI_DP_CONTRACT,
            SenderAddr = executeContract.Value.Sender
        };

        return entity;
    }

    private async Task HandleGatewayPoolInsertAsync(
        IDbConnection db, TerraLiquidityPoolEntity entity,
        CancellationToken cancellationToken)
    {
        // If a swap is _asking_ for bPSI someone is buying bPSI
        if (entity.AskAsset.EqualsIgnoreCase(TerraTokenContracts.BPSI_DP_24M))
        {
            await db.InsertAsync(new TerraPylonPoolEntity
            {
                Id = _idGenerator.Snowflake(),
                Amount = entity.AskAmount,
                Depositor = entity.SenderAddr,
                FriendlyName = TerraPylonPoolFriendlyName.Nexus,
                CreatedAt = entity.CreatedAt,
                Operation = TerraPylonPoolOperation.Buy,
                PoolContract = TerraPylonGatewayContracts.NEXUS,
                TransactionId = entity.TransactionId,
                Denominator = TerraDenominators.TryGetDenominator(entity.OfferAsset)
            }, token: cancellationToken);

            return;
        }

        // If a swap is not asking for BPSI they must be selling it
        if (entity.OfferAsset.EqualsIgnoreCase(TerraTokenContracts.BPSI_DP_24M))
        {
            await db.InsertAsync(new TerraPylonPoolEntity
            {
                Id = _idGenerator.Snowflake(),
                Amount = -1 * entity.OfferAmount,
                Depositor = entity.SenderAddr,
                Operation = TerraPylonPoolOperation.Sell,
                CreatedAt = entity.CreatedAt,
                FriendlyName = TerraPylonPoolFriendlyName.Nexus,
                PoolContract = TerraPylonGatewayContracts.NEXUS,
                TransactionId = entity.TransactionId,
                Denominator = TerraDenominators.AssetTokenAddressToDenominator[entity.OfferAsset],
            }, token: cancellationToken);

            return;
        }

        throw new InvalidOperationException("Neither asking or offering bPSI. Logic flaw");
    }
}