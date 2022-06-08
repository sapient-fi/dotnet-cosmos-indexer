using System.Text;
using MassTransit;
using Microsoft.Extensions.Logging;
using Sapient.Kernel.Contracts.Terra;
using Sapient.Kernel.DAL.Entities.Terra;
using Sapient.Kernel.IdGeneration;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using TerraDotnet;
using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd;
using TerraDotnet.TerraFcd.Messages;
using TerraDotnet.TerraFcd.Messages.Wasm;

namespace Sapient.Infrastructure.Consumers.Terra;

public class LiquidityPoolPairTransactionConsumer : IConsumer<TerraLiquidityPoolPairTransactionMessage>
{
    private readonly ILogger<LiquidityPoolPairTransactionConsumer> _logger;
    private readonly IDbConnectionFactory _dbFactory;
    private readonly IdGenerator _idGenerator;

    public LiquidityPoolPairTransactionConsumer(
        ILogger<LiquidityPoolPairTransactionConsumer> logger,
        IDbConnectionFactory dbFactory,
        IdGenerator idGenerator
    )
    {
        _logger = logger;
        _dbFactory = dbFactory;
        _idGenerator = idGenerator;
    }

    public async Task Consume(ConsumeContext<TerraLiquidityPoolPairTransactionMessage> context)
    {
        using var db = _dbFactory.OpenDbConnection();
        using var tx = db.OpenTransaction();
        var terraTxId = context.Message.TransactionId;
        var terraDbTx = await db.SingleByIdAsync<TerraRawTransactionEntity>(terraTxId);

        var exists = await db.SingleAsync<long?>(db.From<TerraLiquidityPoolPairEntity>()
            .Where(q => q.TransactionId == terraTxId)
            .Take(1)
            .Select(e =>
                new
                {
                    e.Id
                }));
        if (exists.HasValue)
        {
            _logger.LogDebug("Transaction already handled {TxId}", terraTxId);
            tx.Rollback();
            return;
        }

        var cancellationToken = context.CancellationToken;
        var terraTx = terraDbTx.RawTx.ToObject<TerraTxWrapper>()!;
        var msg = TerraTransactionValueFactory.GetIt(terraTx);

        var (pairs, rewards) = ParseTransaction(msg, terraTx);

        foreach (var pair in pairs)
        {
            pair.Dex = context.Message.Dex;
            await db.InsertAsync(pair, token: cancellationToken);
        }

        foreach (var reward in rewards)
        {
            await db.InsertAsync(reward, token: cancellationToken);
        }

        tx.Commit();
    }

    public (List<TerraLiquidityPoolPairEntity> pairResults, List<TerraRewardEntity?> rewardResults) ParseTransaction(
        CoreStdTx stdTx, TerraTxWrapper terraTx)
    {
        var pairResults = new List<TerraLiquidityPoolPairEntity>();
        var rewardResults = new List<TerraRewardEntity?>();

        foreach (var msg in stdTx.Messages)
        {
            if (msg is not WasmMsgExecuteContract executeContract)
            {
                _logger.LogDebug("Skipping message on tx with id {TxId} as it is not exec contract", terraTx.Id);
                continue;
            }

            var provide = HandleAsLpFarmingProvide(
                executeContract,
                terraTx
            );
            if (provide != null)
            {
                pairResults.Add(provide);
                continue;
            }

            var withdraw = HandleAsLpFarmingWithdraw(
                executeContract,
                terraTx
            );
            if (withdraw != null)
            {
                pairResults.Add(withdraw);
                continue;
            }

            var rewards = HandleAsFarmRewardWithdraw(
                executeContract,
                terraTx
            );
            if (rewards != null && rewards.Any())
            {
                rewardResults.AddRange(rewards);
            }
        }

        return (pairResults, rewardResults);
    }

    private List<TerraRewardEntity>? HandleAsFarmRewardWithdraw(
        WasmMsgExecuteContract contract,
        TerraTxWrapper tx
    )
    {
        var results = new List<TerraRewardEntity>();
        WasmExecuteMsgWithdraw? msgWithdraw = null;
        if (
            contract.Value is WasmMsgExecuteContractValueCol5 executeValue)
        {
            msgWithdraw = executeValue.ExecuteMessage?.Withdraw;
        }
        else if (contract.Value is WasmMsgExecuteContractValueCol5 executeValueCol4)
        {
            msgWithdraw = executeValueCol4.ExecuteMessage?.Withdraw;
        }
        else
        {
            _logger.LogDebug("Msg is not {Type}", typeof(WasmMsgExecuteContractValueCol5));
            return null;
        }

        if (msgWithdraw == null)
        {
            _logger.LogDebug("Does not contain a withdraw");
            return null;
        }

        if (msgWithdraw.Asset != null || msgWithdraw.Amount != null)
        {
            // when we have an asset defined it's another type of withdraw that we should not handle as LP farming
            _logger.LogDebug("this is a withdraw tx but asset/amount is defined, probably not LP farming");
            return null;
        }

        _logger.LogInformation("Transaction {Id} is LP claim withdraw", tx.Id);

        var contractAddr = contract.Value.Contract;

        var rewardDenominator = contractAddr switch
        {
            // UST-MINE TerraSwap LP Farming
            TerraswapLpFarmingContracts.PYLON_MINE_UST_FARM => TerraDenominators.Mine,
            AstroportLpFarmingContracts.PYLON_MINE_UST_FARM => TerraDenominators.Mine,

            _ => throw new ArgumentOutOfRangeException(nameof(contract),
                $"Address not known as LP farming contract {contractAddr}"),
        };

        var propToFind = "from_contract";
        var events = tx.Logs
            .SelectMany(l => l.Events)
            .Where(e =>
                e.Type.EqualsIgnoreCase(propToFind)
                && e.Attributes.Contains(new TxLogEventAttribute { Key = "action", Value = "withdraw" })
            );

        foreach (var evt in events)
        {
            var amountAttr = evt.Attributes.FirstOrDefault(attr => attr.Key.EqualsIgnoreCase("amount"));
            if (amountAttr == null)
            {
                _logger.LogCritical("Unable to find amount attribute on event logs for tx: {TxHash}",
                    tx.TransactionHash);
                return null;
            }

            var withdrawAmount = amountAttr.Value.ToInt64() / 1_000_000m;

            results.Add(new TerraRewardEntity
            {
                Id = _idGenerator.Snowflake(),
                Amount = withdrawAmount,
                Denominator = rewardDenominator,
                Wallet = contract.Value.Sender,
                FromContract = contractAddr,
                CreatedAt = tx.CreatedAt,
                RewardType = TerraRewardType.LpFarm,
                TransactionId = tx.Id,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
        }

        return results;
    }

    private TerraLiquidityPoolPairEntity? HandleAsLpFarmingProvide(
        WasmMsgExecuteContract contract,
        TerraTxWrapper tx
    )
    {
        var provideLiquid = contract.Value.ExecuteMessage?.AutoStake
                            ?? contract.Value.ExecuteMessage?.ProvideLiquidity;
        if (provideLiquid == null)
        {
            if (contract.Value.ExecuteMessage?.Compound == null)
            {
                _logger.LogDebug("No autostake, provide liquidity or compound in message, not LP farming");
                return null;
            }

            provideLiquid = new WasmExecuteMsgProvideLiquidity();
        }
    
        if (provideLiquid.Assets == null || provideLiquid.Assets.IsEmpty())
        {
            var assetAttribute = tx.Logs.QueryTxLogsForAttributeFirstOrDefault("from_contract", "assets");
            var amountStrings = assetAttribute.Value.Split(",");
            var assets = amountStrings.Select(amt => amt.ToTerraStringAmount()).ToList();
            provideLiquid.Assets = new List<WasmExecuteMessageAsset>
            {
                assets[0].ToWasmExecuteMessageAsset(),
                assets[1].ToWasmExecuteMessageAsset(),
            };
        }

        var amounts = new List<TerraAmount>
        {
            new(provideLiquid.Assets[0].Amount,
                provideLiquid.Assets[0].Info.NativeToken?.Denominator.FromNativeDenomToDenom()
                ?? TerraDenominators.AssetTokenAddressToDenominator[
                    provideLiquid.Assets[0].Info.Token.ContractAddress]),
            new(provideLiquid.Assets[1].Amount,
                provideLiquid.Assets[1].Info.NativeToken?.Denominator.FromNativeDenomToDenom()
                ?? TerraDenominators.AssetTokenAddressToDenominator[
                    provideLiquid.Assets[1].Info.Token.ContractAddress])
        };
        amounts.EnsureUstIsLast();
        if (!amounts[1].Denominator.Equals(TerraDenominators.Ust))
        {
            _logger.LogCritical("Handling LP pairs that are NON ust is not currently supported");
            throw new NotSupportedException("Handling LP pairs that are NON ust is not currently supported");
        }

        var amountsStr = new[] { provideLiquid.Assets[0].Amount, provideLiquid.Assets[1].Amount };
        var propToFind = "from_contract";
        var lpAmountAttr = tx.Logs
            .QueryTxLogsForAttributeFirstOrDefault(propToFind,
                attr => attr.Key == "amount"
                        && !amountsStr.Contains(attr.Value)
            );


        if (lpAmountAttr == default)
        {
            throw new OperationCanceledException("Unable to find LP Amount");
        }

        return new TerraLiquidityPoolPairEntity
        {
            Id = _idGenerator.Snowflake(),
            TransactionId = tx.Id,
            Farm = $"{amounts[0].Denominator}-{amounts[1].Denominator}",
            AssetOneDenominator = amounts[0].Denominator,
            AssetOneQuantity = amounts[0].Value,
            AssetOneUstValue =
                amounts[1].Value, // Given that it's a UST bound LP pair, asset 1's UST value must be the same as the UST bonded
            AssetTwoDenominator = amounts[1].Denominator,
            AssetTwoQuantity = amounts[1].Value,
            AssetTwoUstValue = amounts[1].Value, // TODO This only works for UST pairs
            AssetLpQuantity = lpAmountAttr.Value.ToInt64() / 1_000_000m,
            CreatedAt = tx.CreatedAt,
            Wallet = contract.Value.Sender
        };
    }

    private TerraLiquidityPoolPairEntity? HandleAsLpFarmingWithdraw(
        WasmMsgExecuteContract contract,
        TerraTxWrapper tx
    )
    {
        if (contract.Value.ExecuteMessage?.Send?.Message == null)
        {
            _logger.LogDebug("No send.message - not a LP withdrawal");
            return null;
        }

        var sendMessage =
            Encoding.UTF8.GetString(Convert.FromBase64String(contract.Value.ExecuteMessage.Send?.Message));
        if (!sendMessage.Contains("withdraw_liquidity"))
        {
            _logger.LogDebug("The send.message contained: {Msg} and not `withdraw_liquidity`, not a LP withdraw",
                sendMessage);
            return null;
        }

        // This is a LP withdrawal, find the assets withdrawn
        var propToFind = "from_contract";
        var lpwithdrawAmountAttr = tx.Logs
            .SelectMany(l =>
                l.Events)
            .Where(e => e.Type.EqualsIgnoreCase(propToFind))
            .SelectMany(evt => evt.Attributes)
            .FirstOrDefault(
                attr => attr.Key == "withdrawn_share"
            );

        if (lpwithdrawAmountAttr == null)
        {
            _logger.LogCritical("Unable to find lp withdrawal amount");
            return null;
        }

        var assetsWithdrawnAttr = tx.Logs
            .SelectMany(l => l.Events)
            .Where(e => e.Type.EqualsIgnoreCase(propToFind))
            .SelectMany(evt => evt.Attributes)
            .FirstOrDefault(attr => attr.Key == "refund_assets");
        if (assetsWithdrawnAttr == default)
        {
            _logger.LogCritical("Unable to find refunded asset");
            return null;
        }

        var lpAmount = lpwithdrawAmountAttr.Value.ToInt64() / 1_000_000m;
        var assetStrings = assetsWithdrawnAttr.Value.Split(',');
        if (assetStrings.Length != 2)
        {
            _logger.LogCritical("Asset withdrawn string seems malformed: {Withdraw}", assetsWithdrawnAttr.Value);
            return null;
        }

        var assets = new List<TerraAmount>(2);
        foreach (var asset in assetStrings)
        {
            assets.Add(asset.ToTerraAmount());
        }

        assets.EnsureUstIsLast();

        return new TerraLiquidityPoolPairEntity
        {
            Id = _idGenerator.Snowflake(),
            Farm = $"{assets[0].Denominator}-{assets[1].Denominator}",
            CreatedAt = tx.CreatedAt,
            TransactionId = tx.Id,
            AssetLpQuantity = -lpAmount,
            AssetOneDenominator = assets[0].Denominator,
            AssetOneQuantity = -assets[0].Value,
            AssetOneUstValue = -assets[1].Value, // TODO only works for UST pairs
            AssetTwoDenominator = assets[1].Denominator,
            AssetTwoQuantity = -assets[1].Value,
            AssetTwoUstValue = -assets[1].Value, // TODO only works for UST pairs
            Wallet = contract.Value.Sender,
        };
    }
}