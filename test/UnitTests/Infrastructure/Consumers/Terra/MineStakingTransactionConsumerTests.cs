using System.IO;
using System.Linq;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Sapient.Infrastructure.Consumers.Terra;
using Sapient.Kernel.IdGeneration;
using ServiceStack.Data;
using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd;
using TerraDotnet.TerraFcd.Messages;
using Xunit;

namespace UnitTests.Infrastructure.Consumers.Terra;

public class MineStakingTransactionConsumerTests
{
    private readonly IdGenerator _idGenerator;
    private readonly MineStakingTransactionConsumer _consumer;
    private readonly string _testFilesBasePath;

    public MineStakingTransactionConsumerTests()
    {
        _idGenerator = A.Fake<IdGenerator>(opts => opts.Strict());
        _consumer = new MineStakingTransactionConsumer(
            A.Fake<ILogger<MineStakingTransactionConsumer>>(),
            A.Fake<IDbConnectionFactory>(opts => opts.Strict()),
            _idGenerator
        );
        _testFilesBasePath = "./Infrastructure/Consumers/Terra/TestFiles";
    }

    [Fact]
    public void HandleGovStake_works()
    {
        // Arrange
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);
        var json = File.ReadAllText($"{_testFilesBasePath}/mine_gov_stake.json");
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var coreTx = TerraTransactionValueFactory.GetIt(terraTx);

        // Act
        var actuals = _consumer.ProcessTransaction(
            terraTx,
            coreTx
        );

        // Assert
        Assert.Single(actuals);
        var actual = actuals.Single();

        Assert.Equal(1, actual.Id);
        Assert.Equal(3984.27m, actual.Amount);
        Assert.Equal("terra16hw950c7gznakfe5rcuj4dj8v94em9kdw9wak6", actual.Sender);
        Assert.Equal(false, actual.IsBuyBack);
    }

    [Fact]
    public void HandleGovUnstake_works()
    {
        // Arrange
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);
        var json = File.ReadAllText($"{_testFilesBasePath}/mine_gov_unstake.json");
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var coreTx = TerraTransactionValueFactory.GetIt(terraTx);

        // Act
        var actuals = _consumer.ProcessTransaction(
            terraTx,
            coreTx
        );

        // Assert
        Assert.Single(actuals);
        var actual = actuals.Single();

        Assert.Equal(1, actual.Id);
        Assert.Equal(-18000m, actual.Amount);
        Assert.Equal("terra1g0uzl468etgkx0gkts42mg7ly6waqkemw89lsh", actual.Sender);
        Assert.Equal(false, actual.IsBuyBack);
    }

    [Fact]
    public void IgnoresVotes()
    {
        // Arrange
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);
        var json = File.ReadAllText($"{_testFilesBasePath}/mine_gov_vote.json");
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var coreTx = TerraTransactionValueFactory.GetIt(terraTx);

        // Act
        var actuals = _consumer.ProcessTransaction(
            terraTx,
            coreTx
        );

        // Assert
        Assert.Empty(actuals);
    }
}