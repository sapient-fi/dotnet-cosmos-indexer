using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RapidCore.Migration;
using SapientFi.Kernel.Config;
using SapientFi.ServiceHost;
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

ConfigureServices.DoIt(builder);


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
        
        if (config.RunMigrationsDuringBoot)
        {
            var migrator = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
            await migrator.UpgradeAsync();
        }
        else
        {
            logger.Warning("{Setting} is false so will not apply migrations", nameof(config.RunMigrationsDuringBoot));
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