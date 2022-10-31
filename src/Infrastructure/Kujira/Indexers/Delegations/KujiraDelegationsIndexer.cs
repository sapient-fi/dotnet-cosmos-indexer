using System.Text.Json;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using SapientFi.Infrastructure.Indexing;
using SapientFi.Infrastructure.Kujira.BusMessages;
using SapientFi.Infrastructure.Kujira.Indexers.Delegations.Storage;
using SapientFi.Infrastructure.Kujira.Storage;
using SapientFi.Kernel.IdGeneration;
using TerraDotnet;
using TerraDotnet.TerraLcd.Messages;

namespace SapientFi.Infrastructure.Kujira.Indexers.Delegations;

public class KujiraDelegationsIndexer : IIndexer<RawKujiraTransactionAvailableAnnouncement>
{
    private readonly ILogger<KujiraDelegationsIndexer> _logger;
    private readonly KujiraDelegationsRepository _repository;
    private readonly KujiraRawRepository _rawRepository;
    private readonly IdProvider _idProvider;

    public KujiraDelegationsIndexer(
        ILogger<KujiraDelegationsIndexer> logger,
        KujiraDelegationsRepository repository,
        KujiraRawRepository rawRepository,
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

    public async Task Consume(ConsumeContext<RawKujiraTransactionAvailableAnnouncement> context)
    {
        var rawTransaction = await _rawRepository.GetByIdOrDefaultAsync(context.Message.RawEntityId, context.CancellationToken);
        if (rawTransaction == default)
        {
            // TODO write to a custom metric in order to be able to monitor missing raw transactions
            _logger.LogWarning("Unable to find raw terra2 transaction with id={RawKujiraId}", context.Message.RawEntityId);
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
                    case CosmosUndelegateMessage undelegateMsg:
                    {
                        if (undelegateMsg.Amount == default)
                        {
                            _logger.LogWarning("Missing amount on undelegate message (txHash={TerraTxHash})", txHash);
                            break;

                        }
                        var undelegateEntity = new KujiraValidatorDelegationLedgerEntity {
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

                    case CosmosDelegateMessage delegateMsg:
                    {
                        if (delegateMsg.Amount == default)
                        {
                            _logger.LogWarning("Missing amount on delegate message (txHash={TerraTxHash})", txHash);
                            break;
                        }

                        var delegateEntity = new KujiraValidatorDelegationLedgerEntity
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

                    case CosmosRedelegateMessage redelegateMsg:
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
                            new KujiraValidatorDelegationLedgerEntity
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
                            new KujiraValidatorDelegationLedgerEntity
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