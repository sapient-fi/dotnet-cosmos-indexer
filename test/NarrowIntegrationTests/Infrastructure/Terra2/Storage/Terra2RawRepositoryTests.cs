using System.Threading;
using System.Threading.Tasks;
using SapientFi.Infrastructure.Terra2.Storage;
using Xunit;

namespace NarrowIntegrationTests.Infrastructure.Terra2.Storage;

public class Terra2RawRepositoryTests : IntegrationBaseTest
{
    private readonly Terra2RawRepository _repository;

    public Terra2RawRepositoryTests()
    {
        _repository = new Terra2RawRepository(GetDbConnectionFactory());
    }
    
    /* ************************************************************************************************************
     */
    [Fact]
    public async Task GetLatestSeenBlockHeightAsync_returns_0_whenTableIsEmpty()
    {
        await EnsureEmptyAsync<Terra2RawTransactionEntity>();

        var actual = await _repository.GetLatestSeenBlockHeightAsync(CancellationToken.None);
        
        Assert.Equal(0, actual);
    }
    
    
    /* ************************************************************************************************************
     */
    [Fact]
    public async Task GetLatestSeenBlockHeightAsync_returns_maxHeight_whenTableHasRows()
    {
        await EnsureEmptyAsync<Terra2RawTransactionEntity>();

        await InsertAsync(new Terra2RawTransactionEntity
        {
            Id = 1,
            Height = 1,
            TxHash = "1"
        });
        
        await InsertAsync(new Terra2RawTransactionEntity
        {
            Id = 2,
            Height = 666,
            TxHash = "2"
        });
        
        await InsertAsync(new Terra2RawTransactionEntity
        {
            Id = 3,
            Height = 2,
            TxHash = "3"
        });

        var actual = await _repository.GetLatestSeenBlockHeightAsync(CancellationToken.None);
        
        Assert.Equal(666, actual);
    }
}