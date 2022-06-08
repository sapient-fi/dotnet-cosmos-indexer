using System;
using System.IO;
using System.Linq;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Sapient.Infrastructure.Consumers.Terra;
using Sapient.Kernel.IdGeneration;
using ServiceStack.Data;
using TerraDotnet;
using TerraDotnet.Extensions;
using TerraDotnet.TerraFcd;
using TerraDotnet.TerraFcd.Messages;
using Xunit;

namespace UnitTests.Infrastructure.Consumers.Terra;

public class LiquidityPoolPairConsumerTests
{
    private readonly IdGenerator _idGenerator;
    private readonly LiquidityPoolPairTransactionConsumer _consumer;
    private readonly string _testFilesBasePath;

    public LiquidityPoolPairConsumerTests()
    {
        _idGenerator = A.Fake<IdGenerator>(opts => opts.Strict());
        _consumer = new LiquidityPoolPairTransactionConsumer(
            A.Fake<ILogger<LiquidityPoolPairTransactionConsumer>>(),
            A.Fake<IDbConnectionFactory>(opts => opts.Strict()),
            _idGenerator
        );
        _testFilesBasePath = "./Infrastructure/Consumers/Terra/TestFiles";
    }

    [Fact]
    public void Handles_astroport_provide()
    {
        /*
            Execute Provide Liquidity on MINE-UST PairMINE-UST Pair

            Provide 635.578942 MINE 24.924582 UST Liquidity to MINE-UST PairMINE-UST Pair
            Mint 125.512909 MINE-UST LP
            Stake 125.512909 MINE-UST LP
         */
        // Arrange
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);
        var json = File.ReadAllText($"{_testFilesBasePath}/astroport_mine_lp_pair_provide.json");
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var coreTx = TerraTransactionValueFactory.GetIt(terraTx);

        // Act
        var (pairResults, rewardResults) = _consumer.ParseTransaction(
            coreTx,
            terraTx
        );

        // Assert
        Assert.Single(pairResults);
        var actual = pairResults.Single();


        Assert.Equal(125.512909m, actual.AssetLpQuantity);

        Assert.Equal(635.578942m, actual.AssetOneQuantity);
        Assert.Equal(24.924582m, actual.AssetOneUstValue);
        Assert.Equal(TerraDenominators.Mine, actual.AssetOneDenominator);

        Assert.Equal(24.924582m, actual.AssetTwoQuantity);
        Assert.Equal(24.924582m, actual.AssetTwoUstValue);
        Assert.Equal(TerraDenominators.Ust, actual.AssetTwoDenominator);
    }

    [Fact]
    public void Handles_astroport_withdraw()
    {
        /*
            Execute Withdraw Liquidity 13,479.838506 MINE-UST LP on MINE-UST PairMINE-UST Pair
            Withdraw 68,192.130514 MINE 2,679.505397 UST Liquidity from MINE-UST PairMINE-UST Pair
            Burn 13,479.838506 MINE-UST LP
         */
        
        A.CallTo(() => _idGenerator.Snowflake()).Returns(1);
        var json = File.ReadAllText($"{_testFilesBasePath}/astroport_mine_lp_pair_withdraw.json");
        var terraTx = json.ToObject<TerraTxWrapper>()!;
        var coreTx = TerraTransactionValueFactory.GetIt(terraTx);

        // Act
        var (pairResults, rewardResults) = _consumer.ParseTransaction(
            coreTx,
            terraTx
        );

        // Assert
        Assert.Single(pairResults);
        var actual = pairResults.Single();

        Assert.Equal(-13479.838506m, actual.AssetLpQuantity);

        Assert.Equal(-68192.130514m, actual.AssetOneQuantity);
        Assert.Equal(-2679.505397m, actual.AssetOneUstValue);
        Assert.Equal(TerraDenominators.Mine, actual.AssetOneDenominator);

        Assert.Equal(-2679.505397m, actual.AssetTwoQuantity);
        Assert.Equal(-2679.505397m, actual.AssetTwoUstValue);
        Assert.Equal(TerraDenominators.Ust, actual.AssetTwoDenominator);
    }
}