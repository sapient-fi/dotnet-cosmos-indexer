using RapidCore.Migration;
using SapientFi.Kernel.Config;
using SapientFi.ServiceHost;
using SapientFi.ServiceHost.Config;
using SapientFi.ServiceHost.Endpoints;
using SapientFi.ServiceHost.ServiceCollectionExtensions;
using Serilog;

ServiceStack.Licensing.RegisterLicense("OSS MIT 2022 https://github.com/pylonboard/dotnet-pylonboard-monolith UgSDQqrNk1uD12MhR04DQogTC/hqGtwW44By+pXTmOl/YuMFf6fINN9DVFJwQU/zoZ8AkM1phq/soc95pHgfqWBMIzAQ1XTC7mUPfHcHxzGSb9am9ps46Y//YpVGQ07mXQxKhmdzYy2nP7Z8CNffS3YaFxOmOQJQO5FZ0dH4988=");

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
builder.Services.AddSignalR();


builder.Services.AddCors(options =>
{
    var config = new CosmosIndexerConfig(builder.Configuration) as ICorsConfig;
    
    options.AddPolicy(name: "cosmos-indexer-cors",
        corsPolicyBuilder =>
        {
            corsPolicyBuilder
                .WithOrigins(config.AllowedOrigins.ToArray())
                .AllowCredentials()
                .WithMethods(new []{"GET", "POST", "PUT", "HEAD", "CONNECT", "OPTIONS", "TRACE"})
                .WithHeaders(new []
                {
                    "accept", "Access-Control-Request-Method", "Access-Control-Request-Headers", "origin", "User-agent", 
                    "Referer", "Accept-Encoding", "Accept-Language", "connection", "host", "content-type",
                    "GraphQL-Tracing", "X-Apollo-Tracing", "newrelic", "traceparent", "tracestate"
                });
        });
});
builder.Services.AddTerraMoney(builder.Configuration);
builder.Services.AddMessageBus(new CosmosIndexerConfig(builder.Configuration), builder.Configuration);
builder.Services.AddDbStack(builder.Configuration);
builder.Services.AddBackgroundJobStack(builder.Configuration);
builder.Services.AddEndpointServices();

builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddDiagnosticEventListener<NewRelicReportingDiagnosticEventListener>()
    .AddApolloTracing(); // https://chillicream.com/docs/hotchocolate/server/instrumentation#on-demand

var app = builder.Build();
app.UseCors("cosmos-indexer-cors");
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health");
    endpoints.MapGraphQL();
    endpoints.MapControllers();
});
try
{
    var logger = Log.ForContext(typeof(Program));
    using (var scope = app.Services.CreateScope())
    {
        var config = scope.ServiceProvider.GetRequiredService<IDbConfig>();
        
        if (config.DisableMigrationsDuringBoot)
        {
            logger.Warning("{Setting} is false so will not apply migrations", nameof(config.DisableMigrationsDuringBoot));
        }
        else
        {
            var migrator = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
            await migrator.UpgradeAsync();
        }
        var cronJobs = app.Services.GetRequiredService<CronjobManager>();
        cronJobs.RegisterJobsIfRequired();
    }
    
    app.Run();
}
catch (Exception e)
{
    Log.ForContext("FatalException", e)
        .Fatal(e, "Application has crashed unexpectedly!");
    throw;
}
finally
{
    Log.CloseAndFlush();
}