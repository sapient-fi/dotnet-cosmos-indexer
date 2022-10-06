using SapientFi.Infrastructure.Kujira;
using SapientFi.Infrastructure.Terra2;
using SapientFi.Kernel.Config;
using SapientFi.ServiceHost.Config;
using SapientFi.ServiceHost.Endpoints;
using SapientFi.ServiceHost.ServiceCollectionExtensions;

namespace SapientFi.ServiceHost;

public static class ConfigureServices
{
    public static void DoIt(WebApplicationBuilder builder)
    {
        DoIt(builder.Services, builder.Configuration);
    }
    
    public static void DoIt(IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();
        services.AddHealthChecks();
        services.AddSignalR();
        
        services.AddCors(options =>
        {
            var config = new CosmosIndexerConfig(configuration) as ICorsConfig;
    
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
        services.AddTerraMoney(configuration);
        services.AddMessageBus(new CosmosIndexerConfig(configuration), configuration);
        services.AddDbStack(configuration);
        services.AddBackgroundJobStack(configuration);
        services.AddEndpointServices();
        services.AddTerra2Stack(configuration);
        services.AddKujiraStack(configuration);

        services.AddGraphQLServer()
            .AddQueryType<Query>()
            .AddDiagnosticEventListener<NewRelicReportingDiagnosticEventListener>()
            .AddApolloTracing(); // https://chillicream.com/docs/hotchocolate/server/instrumentation#on-demand
    }
}