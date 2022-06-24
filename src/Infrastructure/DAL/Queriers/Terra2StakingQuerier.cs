using SapientFi.Infrastructure.Terra2.Indexers.Delegations.Storage;
using ServiceStack.OrmLite;

namespace SapientFi.Infrastructure.DAL.Queriers;

public class Terra2StakingQuerier
{
    private readonly Terra2DelegationsRepository _repository;

    public Terra2StakingQuerier(Terra2DelegationsRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<(string, long)>> TotalStakingCurrentlyAtSpecificValidatorsAsync(
        IEnumerable<string> validatorAddresses,
        CancellationToken cancellationToken = default)
    {
        using var db = await _repository.GetDbConnectionAsync(cancellationToken);
        var result = await db.SqlListAsync<(string, long)>(db.From<Terra2ValidatorDelegationLedgerEntity>()
                .Select(delegation => 
                    new {
                        ValidatorAddr = delegation.ValidatorAddress,
                        Sum = Sql.Sum(delegation.Amount)
                    })
                .GroupBy(delegation => delegation.ValidatorAddress)
                .Where(delegation => Sql.In(delegation.ValidatorAddress, validatorAddresses)),
            cancellationToken
        );

        return result;
    }

    public async Task<List<(string, long)>> TotalStakingAtGivenTimeAtSpecificValidatorsAsync(
        IEnumerable<string> validatorAddresses,
        DateTime dateTime,
        CancellationToken cancellationToken = default
    )
    {
        using var db = await _repository.GetDbConnectionAsync(cancellationToken);
        
        var result = await db.SqlListAsync<(string, long)>(db.From<Terra2ValidatorDelegationLedgerEntity>()
                .Select(delegation =>
                    new {
                        ValidatorAddr = delegation.ValidatorAddress,
                        Sum = Sql.Sum(delegation.Amount)
                    })
                .GroupBy(delegation => delegation.ValidatorAddress)
                .Where(delegation => Sql.In(delegation.ValidatorAddress, validatorAddresses))
                .And(validator => validator.At <= dateTime),
            cancellationToken
        );

        return result;
    }

    public async Task<List<(string, long)>> DeltaStakingInTimePeriodAtSpecificValidators(
        IEnumerable<string> validatorAddresses,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {

        using var db = await _repository.GetDbConnectionAsync(cancellationToken);
        
        var result = await db.SqlListAsync<(string, long)>(db.From<Terra2ValidatorDelegationLedgerEntity>()
                .Select(delegation => 
                    new {
                        ValidatorAddr = delegation.ValidatorAddress,
                        Sum = Sql.Sum(delegation.Amount)
                    })
                .GroupBy(delegation => delegation.ValidatorAddress)
                .Where(delegation => Sql.In(delegation.ValidatorAddress, validatorAddresses))
                .And(validator => validator.At >= from && validator.At <= to),
            cancellationToken
        );

        return result;
    }
}