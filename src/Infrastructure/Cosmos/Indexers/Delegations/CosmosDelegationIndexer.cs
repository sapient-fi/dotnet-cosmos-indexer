using System.Text.Json;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using SapientFi.Infrastructure.Cosmos.BusMessages;
using SapientFi.Infrastructure.Cosmos.Indexers.Delegations.Storage;
using SapientFi.Infrastructure.Cosmos.Storage;
using SapientFi.Infrastructure.Indexing;
using SapientFi.Kernel.IdGeneration;
using TerraDotnet;
using TerraDotnet.TerraFcd.Messages;
using TerraDotnet.TerraLcd.Messages;

namespace SapientFi.Infrastructure.Cosmos.Indexers.Delegations;

public abstract class
    CosmosDelegationIndexer<
        TRawTransactionAvailableAnnouncement,
        TValidatorDelegationLedgerEntity,
        TRawTransactionEntity>
    : IIndexer<TRawTransactionAvailableAnnouncement>
    where TRawTransactionAvailableAnnouncement : class, IRawCosmosTransactionAvailableAnnouncement
    where TValidatorDelegationLedgerEntity : ICosmosValidatorDelegationLedgerEntity, new()
    where TRawTransactionEntity : ICosmosRawTransactionEntity, new()
{
    protected readonly
        ILogger<CosmosDelegationIndexer<
            TRawTransactionAvailableAnnouncement,
            TValidatorDelegationLedgerEntity,
            TRawTransactionEntity>> Logger;
    protected readonly CosmosDelegationsRepository<TValidatorDelegationLedgerEntity> Repository;
    protected readonly CosmosRawRepository<TRawTransactionEntity> RawRepository;
    protected readonly IdProvider IdProvider;

    protected CosmosDelegationIndexer(
        ILogger<CosmosDelegationIndexer<
            TRawTransactionAvailableAnnouncement,
            TValidatorDelegationLedgerEntity,
            TRawTransactionEntity>> logger,
        CosmosDelegationsRepository<TValidatorDelegationLedgerEntity> repository,
        CosmosRawRepository<TRawTransactionEntity> rawRepository,
        IdProvider idProvider
    )
    {
        Logger = logger;
        Repository = repository;
        RawRepository = rawRepository;
        IdProvider = idProvider;
    }
    
    public abstract string NameOfBlockChain { get; }
    public abstract string Id { get; }
    public abstract string DisplayName { get; }

    public virtual async Task Consume(ConsumeContext<TRawTransactionAvailableAnnouncement> context)
    {
        await DefaultConsume(context);
    }

    protected async Task DefaultConsume(ConsumeContext<TRawTransactionAvailableAnnouncement> context)
    {
        var rawTransaction = await RawRepository.GetByIdOrDefaultAsync(
            context.Message.RawEntityId,
            context.CancellationToken
        );
        if (rawTransaction is null)
        {
            // TODO write to a custom metric in order to be able to monitor missing raw transactions
            Logger.LogWarning(
                "({BlockChain}) Unable to find raw transaction with id={RawEntityId}",
                NameOfBlockChain,
                context.Message.RawEntityId
            );
            return;
        }

        var txHash = rawTransaction.TxHash;

        // TODO: deal with re-entrance/re-processing i.e. make sure we do not make duplicate entries if we process the same TX multiple times

        var tx = JsonSerializer.Deserialize<LcdTxResponse>(rawTransaction.RawTx);
        if (tx == default)
        {
            Logger.LogWarning(
                "({BlockChain}) Could not deserialize transaction with hash={TxHash}",
                NameOfBlockChain,
                txHash
            );
        }

        foreach (var txMessage in tx!.Transaction.Body.Messages)
        {
            var parseSuccessful = TerraMessageParser.TryParse(txMessage, out IMsg? message);
            if (parseSuccessful)
            {
                switch (message)
                {
                    case CosmosUndelegateMessage undelegateMsg:
                    {
                        if (undelegateMsg.Amount == default)
                        {
                            Logger.LogWarning(
                                "({BlockChain}) Missing amount on undelegate message (txHash={TxHash})",
                                NameOfBlockChain,
                                txHash
                            );
                            break;
                        }

                        var undelegateEntity = new TValidatorDelegationLedgerEntity
                        {
                            Id = IdProvider.Snowflake(),
                            TxHash = txHash,
                            At = rawTransaction.CreatedAt,
                            DelegatorAddress = undelegateMsg.DelegatorAddress,
                            ValidatorAddress = undelegateMsg.ValidatorAddress,
                            Amount = -long.Parse(undelegateMsg.Amount.Amount), // subtracts amount
                            Denominator = undelegateMsg.Amount.Denominator
                        };

                        await Repository.SaveAsync(undelegateEntity, context.CancellationToken);
                        break;
                    }

                    case CosmosDelegateMessage delegateMsg:
                    {
                        if (delegateMsg.Amount == default)
                        {
                            Logger.LogWarning(
                                "({BlockChain}) Missing amount on delegate message (txHash={TxHash})",
                                NameOfBlockChain,
                                txHash
                            );
                            break;
                        }

                        var delegateEntity = new TValidatorDelegationLedgerEntity
                        {
                            Id = IdProvider.Snowflake(),
                            TxHash = txHash,
                            At = rawTransaction.CreatedAt,
                            DelegatorAddress = delegateMsg.DelegatorAddress,
                            ValidatorAddress = delegateMsg.ValidatorAddress,
                            Amount = long.Parse(delegateMsg.Amount.Amount), // adds amount
                            Denominator = delegateMsg.Amount.Denominator
                        };

                        await Repository.SaveAsync(delegateEntity, context.CancellationToken);
                        break;
                    }

                    case CosmosRedelegateMessage redelegateMsg:
                    {
                        if (redelegateMsg.Amount == default)
                        {
                            Logger.LogWarning(
                                "({BlockChain}) Missing amount on redelegate message (txHash={TxHash})",
                                NameOfBlockChain,
                                txHash
                            );
                            break;
                        }

                        var entities = new[]
                        {
                            // first a ledger entry that removes the delegation
                            // from the original validator
                            new TValidatorDelegationLedgerEntity
                            {
                                Id = IdProvider.Snowflake(),
                                TxHash = txHash,
                                At = rawTransaction.CreatedAt,
                                DelegatorAddress = redelegateMsg.DelegatorAddress,
                                ValidatorAddress = redelegateMsg.ValidatorSourceAddress,
                                Amount = -long.Parse(redelegateMsg.Amount.Amount),  // subtracts amount
                                Denominator = redelegateMsg.Amount.Denominator
                            },
                            // then a ledger entry adding the delegation to the new validator
                            new TValidatorDelegationLedgerEntity
                            {
                                Id = IdProvider.Snowflake(),
                                TxHash = txHash,
                                At = rawTransaction.CreatedAt,
                                DelegatorAddress = redelegateMsg.DelegatorAddress,
                                ValidatorAddress = redelegateMsg.ValidatorDestinationAddress,
                                Amount = long.Parse(redelegateMsg.Amount.Amount),  // adds amount
                                Denominator = redelegateMsg.Amount.Denominator
                            }
                        };

                        await Repository.SaveAllAsync(entities, context.CancellationToken);
                        break;
                    }

                    default:
                        // we do not care about this message, so leave it be :)
                        break;
                }
            }
        }
    }
}
