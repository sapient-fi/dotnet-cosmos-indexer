using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Sapient.Infrastructure.Consumers.Terra;
using Sapient.Kernel.DAL.Entities.Terra;
using Sapient.Kernel.IdGeneration;
using ServiceStack.Data;
using TerraDotnet;
using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd.Messages;
using Xunit;

namespace UnitTests.Infrastructure.Consumers.Terra;

public class PylonGatewayPoolTransactionConsumerTests
{
    private readonly IdGenerator _idGenerator;
    private readonly PylonGatewayPoolTransactionConsumer _consumer;
    private string _testFilesBasePath;

    public PylonGatewayPoolTransactionConsumerTests()
    {
        _idGenerator = A.Fake<IdGenerator>(opts => opts.Strict());
        _consumer = new PylonGatewayPoolTransactionConsumer(
            A.Fake<ILogger<PylonGatewayPoolTransactionConsumer>>(),
            A.Fake<IDbConnectionFactory>(opts => opts.Strict()),
            _idGenerator
        );
        _testFilesBasePath = "./Infrastructure/Consumers/Terra/TestFiles";
    }

    [Fact]
    public async Task HandlingDeposit_works()
    {
        // Arrange
        var json = await File.ReadAllTextAsync($"{_testFilesBasePath}/loop_1_deposit.json");
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);

        // act
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var actuals = _consumer.ParseTransaction(
            TerraPylonPoolFriendlyName.Loop1,
            TerraPylonGatewayContracts.LOOP_1,
            terraTx
        );

        // assert
        Assert.Single(actuals);
        var actual = actuals.Single();

        Assert.Equal(1, actual.Id);
        Assert.Equal(100m, actual.Amount);
        Assert.Equal(TerraPylonPoolOperation.Deposit, actual.Operation);
        Assert.Equal(TerraDenominators.Ust, actual.Denominator);
        Assert.Equal(terraTx.CreatedAt, actual.CreatedAt);
    }

    [Fact]
    public async Task HandlingWithdraw_works()
    {
        // Arrange
        var json = await File.ReadAllTextAsync($"{_testFilesBasePath}/loop_3_withdraw.json");
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);

        // act
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var actuals = _consumer.ParseTransaction(
            TerraPylonPoolFriendlyName.Loop3,
            TerraPylonGatewayContracts.LOOP_3,
            terraTx
        );

        // assert
        Assert.Single(actuals);
        var actual = actuals.Single();

        Assert.Equal(1, actual.Id);
        Assert.Equal(-997.160922m, actual.Amount);
        Assert.Equal(TerraPylonPoolOperation.Withdraw, actual.Operation);
        Assert.Equal(TerraDenominators.Ust, actual.Denominator);
        Assert.Equal(terraTx.CreatedAt, actual.CreatedAt);
    }
    
    [Fact]
    public async Task HandlingClaim_works()
    {
        // Arrange
        var json = await File.ReadAllTextAsync($"{_testFilesBasePath}/loop_3_claim.json");
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);

        // act
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var actuals = _consumer.ParseTransaction(
            TerraPylonPoolFriendlyName.Loop3,
            TerraPylonGatewayContracts.LOOP_3,
            terraTx
        );

        // assert
        Assert.Single(actuals);
        var actual = actuals.Single();

        Assert.Equal(1, actual.Id);
        Assert.Equal(233.818782m, actual.Amount);
        Assert.Equal(TerraPylonPoolOperation.Claim, actual.Operation);
        Assert.Equal(TerraDenominators.Loop, actual.Denominator);
        Assert.Equal(terraTx.CreatedAt, actual.CreatedAt);
    }
    
    [Fact]
    public async Task HandingNonGwAction_is_graceful()
    {
        // Arrange
        var json = await File.ReadAllTextAsync($"{_testFilesBasePath}/ancust_lp_deposit.json");
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);

        // act
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        
        var ex = Record.Exception(() => _consumer.ParseTransaction(
            TerraPylonPoolFriendlyName.Loop3,
            TerraPylonGatewayContracts.LOOP_3,
            terraTx
        ));

        // assert
        Assert.Null(ex);
    }
}