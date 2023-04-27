using System;
using SapientFi.Infrastructure.Cosmos.Indexers.Delegations.Storage;
using ServiceStack.DataAnnotations;

namespace SapientFi.Infrastructure.Terra2.Indexers.Delegations.Storage;

/// <summary>
/// Represents an entry in a "ledger" of delegated
/// governance tokens for a specific validator.
///
/// This means that when tokens are re-delegated
/// i.e. moved from validator A to validator B,
/// we have 2 entries for the same action (but with
/// inverted signs), as the tokens are leaving A
/// and going into B. 
/// </summary>
public record Terra2ValidatorDelegationLedgerEntity : ICosmosValidatorDelegationLedgerEntity
{
    public long Id { get; init; }
    
    public DateTimeOffset At { get; init; }

    /// <summary>
    /// The TxHash of the transaction that this delegation
    /// came from
    /// </summary>
    [Index]
    public string TxHash { get; init; } = string.Empty;

    [Index]
    public string ValidatorAddress { get; init; } = string.Empty;

    [Index]
    public string DelegatorAddress { get; init; } = string.Empty;
    
    /// <summary>
    /// Positive = delegation to validator
    /// Negative = delegation away from validator
    /// </summary>
    public long Amount { get; init; }

    public string Denominator { get; init; } = string.Empty;
}