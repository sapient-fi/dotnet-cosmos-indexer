using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using SapientFi.Infrastructure.Indexing;
using SapientFi.Infrastructure.Terra2.BusMessages;
using SapientFi.Infrastructure.Terra2.Indexers.Delegations.Storage;
using SapientFi.Infrastructure.Terra2.Storage;
using SapientFi.Kernel.IdGeneration;
using TerraDotnet;
using TerraDotnet.TerraLcd.Messages;

namespace SapientFi.Infrastructure.Terra2.Indexers.Delegations;

public class Terra2DelegationsIndexer : IIndexer<RawTerra2TransactionAvailableAnnouncement>
{
    private readonly ILogger<Terra2DelegationsIndexer> _logger;
    private readonly Terra2DelegationsRepository _repository;
    private readonly Terra2RawRepository _rawRepository;
    private readonly IdProvider _idProvider;

    public Terra2DelegationsIndexer(
        ILogger<Terra2DelegationsIndexer> logger,
        Terra2DelegationsRepository repository,
        Terra2RawRepository rawRepository,
        IdProvider idProvider
    )
    {
        _logger = logger;
        _repository = repository;
        _rawRepository = rawRepository;
        _idProvider = idProvider;
    }

    public string Id => "terra2_delegations";

    public string DisplayName => "Terra 2 Delegations";

    public async Task Consume(ConsumeContext<RawTerra2TransactionAvailableAnnouncement> context)
    {
        var rawTransaction = await _rawRepository.GetByIdOrDefaultAsync(context.Message.RawEntityId, context.CancellationToken);
        if (rawTransaction == default)
        {
            // TODO write to a custom metric in order to be able to monitor missing raw transactions
            _logger.LogWarning("Unable to find raw terra2 transaction with id={RawTerra2Id}", context.Message.RawEntityId);
            return;
        }

        var txHash = rawTransaction.TxHash;
        
        // TODO deal with re-entrance/re-processing i.e. make sure we do not make duplicate entries if we process the same TX multiple times


        var tx = JsonSerializer.Deserialize<LcdTxResponse>(rawTransaction.RawTx);
        if (tx == default)
        {
            _logger.LogWarning("Could not deserialize transaction with hash={TxHash}", rawTransaction.TxHash);
        }
        
        foreach (var txMessage in tx!.Transaction.Body.Messages)
        {
            var parseSuccessful = TerraMessageParser.TryParse(txMessage, out var message);
            if (parseSuccessful)
            {
                switch (message)
                {
                    case Terra2UndelegateMessage undelegateMsg:
                    {
                        if (undelegateMsg.Amount == default)
                        {
                            _logger.LogWarning("Missing amount on undelegate message (txHash={TerraTxHash})", txHash);
                            break;

                        }
                        var undelegateEntity = new Terra2ValidatorDelegationLedgerEntity {
                            Id = _idProvider.Snowflake(),
                            TxHash = txHash,
                            At = rawTransaction.CreatedAt,
                            DelegatorAddress = undelegateMsg.DelegatorAddress,
                            ValidatorAddress = undelegateMsg.ValidatorAddress,
                            Amount = -long.Parse(undelegateMsg.Amount.Amount),
                            Denominator = undelegateMsg.Amount.Denominator
                        };

                        await _repository.SaveAsync(undelegateEntity, context.CancellationToken);
                        break;
                    }

                    case Terra2DelegateMessage delegateMsg:
                    {
                        if (delegateMsg.Amount == default)
                        {
                            _logger.LogWarning("Missing amount on delegate message (txHash={TerraTxHash})", txHash);
                            break;
                        }

                        var delegateEntity = new Terra2ValidatorDelegationLedgerEntity
                        {
                            Id = _idProvider.Snowflake(),
                            TxHash = txHash,
                            At = rawTransaction.CreatedAt,
                            DelegatorAddress = delegateMsg.DelegatorAddress,
                            ValidatorAddress = delegateMsg.ValidatorAddress,
                            Amount = long.Parse(delegateMsg.Amount.Amount),
                            Denominator = delegateMsg.Amount.Denominator
                        };

                        await _repository.SaveAsync(delegateEntity, context.CancellationToken);
                        break;
                    }

                    case Terra2RedelegateMessage redelegateMsg:
                    {
                        if (redelegateMsg.Amount == default)
                        {
                            _logger.LogWarning("Missing amount on redelegate message (txHash={TerraTxHash})", txHash);
                            break;
                        }

                        var entities = new[]
                        {
                            // first a ledger entry that removes the delegation
                            // from the original validator
                            new Terra2ValidatorDelegationLedgerEntity
                            {
                                Id = _idProvider.Snowflake(),
                                TxHash = txHash,
                                At = rawTransaction.CreatedAt,
                                DelegatorAddress = redelegateMsg.DelegatorAddress,
                                ValidatorAddress = redelegateMsg.ValidatorSourceAddress,
                                Amount = -long.Parse(redelegateMsg.Amount.Amount),
                                Denominator = redelegateMsg.Amount.Denominator
                            },
                            // then a ledger entry adding the delegation to the new validator
                            new Terra2ValidatorDelegationLedgerEntity
                            {
                                Id = _idProvider.Snowflake(),
                                TxHash = txHash,
                                At = rawTransaction.CreatedAt,
                                DelegatorAddress = redelegateMsg.DelegatorAddress,
                                ValidatorAddress = redelegateMsg.ValidatorDestinationAddress,
                                Amount = long.Parse(redelegateMsg.Amount.Amount),
                                Denominator = redelegateMsg.Amount.Denominator
                            }
                        };

                        await _repository.SaveAllAsync(entities, context.CancellationToken);
                        break;
                    }

                    default:
                        // we do not care about this message, so leave it be :)
                        break;
                }
            }
        }
        //*/
    }
}