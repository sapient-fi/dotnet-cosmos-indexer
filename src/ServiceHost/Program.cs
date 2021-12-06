using Pylonboard.ServiceHost;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.Extensions;
using Pylonboard.ServiceHost.ServiceCollectionExtensions;
using RapidCore.Migration;
using Serilog;

var builder = WebApplication
    .CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();
builder.Host.UseSerilog();


// Add services to the container.
var loggerConfiguration = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration);

Log.Logger = loggerConfiguration.CreateLogger();

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
        // .AddCheck<MarketDataHealthCheck>("market_data_health_check")
        // .AddNpgSql(dbConfig.ConnectionString);
        // .AddRedis(Configuration["Data:ConnectionStrings:Redis"]);

builder.Services.AddTerraMoney(builder.Configuration);
builder.Services.AddMessageBus(new PylonboardConfig(builder.Configuration), builder.Configuration);
builder.Services.AddDbStack(builder.Configuration);

var app = builder.Build();
app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHealthChecks("/health");
});
try
{
    using (var scope = app.Services.CreateScope())
    {
        var migrator = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
        await migrator.UpgradeAsync();
    }
    
    app.Run();
}
catch (Exception e)
{
    Log.ForContext("FatalException", e)
        .Fatal(e, "Application has crashed unexpectedly!");
    // SentrySdk.CaptureException(e);
    throw;
}
finally
{
    Log.CloseAndFlush();
}