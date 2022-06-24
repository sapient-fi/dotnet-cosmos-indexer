using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using SapientFi.Infrastructure.Terra2.Indexers.Delegations.Storage;
using SapientFi.Infrastructure.Terra2.Storage;
using ServiceStack.OrmLite;
using TerraDotnet;
using TerraDotnet.TerraLcd.Messages;
using Xunit;

namespace NarrowIntegrationTests.TerraDotnet;

public class Terra2StakingQuerierTests : IntegrationBaseTest
{
    private readonly Terra2RawRepository _rawRepository;
    private readonly Terra2DelegationsRepository _delegationsRepository;

    public Terra2StakingQuerierTests()
    {
        _rawRepository = new Terra2RawRepository(GetDbConnectionFactory());
        _delegationsRepository = new Terra2DelegationsRepository(GetDbConnectionFactory());
    }
    
    private GetTransactionsMatchingQueryResponse? GetParsedJson()
    {
        var json = File.ReadAllText("TerraDotnet/GetTransactionsMatchingQueryResponse_02.json");

        return JsonSerializer.Deserialize<GetTransactionsMatchingQueryResponse>(
            json,
            TerraJsonSerializerOptions.GetThem()
        );
    }

    [Fact]
    public async void gfjdghjd()
    {
        var db = await _delegationsRepository.GetDbConnectionAsync(new CancellationToken());
        var valAddr = "terravaloper1plxp55happ379eevsnk2xeuwzsrldsmqduyu36";

        var theSum = await db.ScalarAsync<long>(
            db.From<Terra2ValidatorDelegationLedgerEntity>()
            .Select(q => Sql.Sum(q.Amount))
            .Where(x => x.ValidatorAddress == valAddr)
        );
    
        Assert.Equal(0L, theSum);
    }
}
