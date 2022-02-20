using System;
using System.IO;
using System.Linq;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Pylonboard.Infrastructure.Consumers.Terra;
using Pylonboard.Kernel.IdGeneration;
using ServiceStack.Data;
using TerraDotnet;
using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd;
using TerraDotnet.TerraFcd.Messages;
using Xunit;

namespace UnitTests.Infrastructure.Consumers.Terra;

public class BpsiDpLiquidityPoolConsumerTests
{
    private readonly IdGenerator _idGenerator;
    private readonly BpsiDpLiquidityPoolConsumer _consumer;
    private readonly string _testFilesBasePath;

    public BpsiDpLiquidityPoolConsumerTests()
    {
        _idGenerator = A.Fake<IdGenerator>(opts => opts.Strict());
        _consumer = new BpsiDpLiquidityPoolConsumer(
            A.Fake<ILogger<BpsiDpLiquidityPoolConsumer>>(),
            A.Fake<IDbConnectionFactory>(opts => opts.Strict()),
            _idGenerator
        );
        _testFilesBasePath = "./Infrastructure/Consumers/Terra/TestFiles";
    }

    [Fact]
    public void HandlesSendMsg_sell_bpsi_to_psi()
    {
        // Arrange
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);
        var json = File.ReadAllText($"{_testFilesBasePath}/bpsi_send_sell_to_psi.json");
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var coreTx = TerraTransactionValueFactory.GetIt(terraTx);

        // Act
        var actuals = _consumer.ParseTransaction(
            coreTx,
            terraTx
        );

        // Assert
        Assert.Single(actuals);
        var actual = actuals.Single();
        
        Assert.Equal(1, actual.Id);
        Assert.Equal(TerraTokenContracts.PSI, actual.AskAsset);
        Assert.Equal(TerraTokenContracts.BPSI_DP_24M, actual.OfferAsset);
        Assert.Equal(37.812648m, actual.OfferAmount);
        Assert.Equal(1246.943361m, actual.AskAmount);
    }

    [Fact]
    public void HandlesSendMsg_with_routeSwap_bpsi_to_ust()
    {
        // Arrange
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);
        var json = File.ReadAllText($"{_testFilesBasePath}/bpsi_route_swap_to_ust.json");
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var coreTx = TerraTransactionValueFactory.GetIt(terraTx);

        // Act
        var actuals = _consumer.ParseTransaction(
            coreTx,
            terraTx
        );

        // Assert
        Assert.Single(actuals);
        var actual = actuals.Single();
        
        Assert.Equal(1, actual.Id);
        Assert.Equal("uusd", actual.AskAsset);
        Assert.Equal(TerraTokenContracts.BPSI_DP_24M, actual.OfferAsset);
        Assert.Equal(2476.117951m, actual.OfferAmount);
        Assert.Equal(2049.764506m, actual.AskAmount);
    }
}