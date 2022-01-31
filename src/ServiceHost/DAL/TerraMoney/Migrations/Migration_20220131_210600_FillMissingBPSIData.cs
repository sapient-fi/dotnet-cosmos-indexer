using Pylonboard.Kernel;
using Pylonboard.ServiceHost.DAL.Exchanges;
using Pylonboard.ServiceHost.Oracles.ExchangeRates.Terra;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace Invacoil.Data.Migrations
{
    public class Migration_20220131_210600_FillMissingBPSIData : MigrationBase
    {
        protected override void ConfigureUpgrade(IMigrationBuilder builder)
        {
            var ctx = ContextAs<PostgreSqlMigrationContext>();
            var connection = ctx.ConnectionProvider.Default();
            var logger = ctx.Container.Resolve<ILogger<Migration_20220131_210600_FillMissingBPSIData>>();
            builder.Step("fill table with missing bPsiDP arb info", async () =>
            {
                var exchangeRateOracle = ctx.Container.Resolve<TerraExchangeRateOracle>();
                // 2022-01-28 12:10:00.000000 +00:00
                var startAt = new DateTimeOffset(2022, 01, 28, 12, 10, 0, TimeSpan.Zero);
                // 2022-01-31 11:20:00.000000 +00:00
                var endAt = new DateTimeOffset(2022, 01, 31, 11, 20, 0, TimeSpan.Zero);

                do
                {
                    logger.LogInformation("Fetching candle at {Time:u}", startAt);
                    var toPsi = await exchangeRateOracle.GetExchangeRateAsync(TerraDenominators.bPsiDP, TerraDenominators.Psi, startAt, "5m");
                    var toUst = await exchangeRateOracle.GetExchangeRateAsync(TerraDenominators.Psi, TerraDenominators.Ust,startAt, "5m");

                    var endResult = toUst.close * toPsi.close;

                    await connection.InsertAsync<ExchangeMarketCandle>(new ExchangeMarketCandle
                    {
                        Close = endResult,
                        Exchange = Exchange.Terra,
                        Market = $"{TerraDenominators.bPsiDP}-arb",
                        CloseTime = toUst.closedAt,
                    });
                    
                    startAt = startAt.AddMinutes(5);
                } while(startAt <= endAt);
            });
        }

        protected override void ConfigureDowngrade(IMigrationBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}