using MassTransit;
using SapientFi.Kernel.Config;
using SapientFi.ServiceHost.Config;

namespace SapientFi.ServiceHost.ServiceCollectionExtensions;

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
                /*
                x.AddConsumers(
                    typeof(MineStakingTransactionConsumer).Assembly
                );
                //*/
            }
                
            x.UsingRabbitMq((context, cfg) =>
            {
                var config = new CosmosIndexerConfig(configuration);

                cfg.Host(new Uri((config as IMessageTransportConfig).TransportUri));

                cfg.ConfigureEndpoints(context);

            });
        });
        services.AddMassTransitHostedService();
        services.AddGenericRequestClient();

        return services;
    }
}