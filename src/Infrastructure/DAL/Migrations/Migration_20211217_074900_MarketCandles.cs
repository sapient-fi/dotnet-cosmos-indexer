using Pylonboard.Infrastructure.Oracles.ExchangeRates.Terra;
using Pylonboard.Kernel;
using Pylonboard.Kernel.DAL.Entities.Exchanges;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;
using TerraDotnet;

namespace Invacoil.Data.Migrations
{
    public class Migration_20211217_074900_MarketCandles : MigrationBase
    {
        protected override void ConfigureUpgrade(IMigrationBuilder builder)
        {
            var ctx = ContextAs<PostgreSqlMigrationContext>();
            var connection = ctx.ConnectionProvider.Default();
            builder.Step("add exchange market candle table", () =>
            {
                connection.Execute(@"
create table exchange_market_candle
(
    id bigserial,
	open numeric(38,6) not null,
	high numeric(38,6) not null,
	low numeric(38,6) not null,
	close numeric(38,6) not null,
	volume numeric(38,6) not null,
	market text,
	open_time timestamp with time zone not null,
	close_time timestamp with time zone not null,
	exchange varchar(255) not null
);
");
            });
            builder.Step("fill table with bPsiDP arb info", async () =>
            {
                var exchangeRateOracle = ctx.Container.Resolve<TerraExchangeRateOracle>();
                var now = DateTimeOffset.Now;
                var startAt = new DateTimeOffset(2021, 12, 17, 14, 15, 0, TimeSpan.Zero);

                do
                {
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
                } while(startAt <= now);
            });
        }

        protected override void ConfigureDowngrade(IMigrationBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}