using MassTransit;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.TerraDataFetchers;

namespace Pylonboard.ServiceHost.ServiceCollectionExtensions;

public static class MessageBusServiceExtensions
{
    public static IServiceCollection AddMessageBus(this IServiceCollection services,
        IEnabledServiceRolesConfig enabledServiceRolesConfig, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();
            // Check this out: https://masstransit-project.com/usage/containers/#consumer-definition
            // Add all consumers in the specified assembly
                
            if (enabledServiceRolesConfig.IsRoleEnabled(ServiceRoles.API))
            {
                x.AddConsumers(
                    typeof(MessageBusServiceExtensions).Assembly
                );
            }

            if (enabledServiceRolesConfig.IsRoleEnabled(ServiceRoles.BACKGROUND_WORKER))
            {
                x.AddConsumers(
                    typeof(TerraAirdropContracts).Assembly
                );
            }
                
            x.UsingGrpc((context, cfg) =>
            {
                cfg.Host(h =>
                {
                    //TODO Hosts from config 
                    h.Host = "127.0.0.1";
                    h.Port = 19796;
                });

                cfg.ConfigureEndpoints(context);
            });
        });
        services.AddMassTransitHostedService();
        services.AddGenericRequestClient();

        return services;
    }
}