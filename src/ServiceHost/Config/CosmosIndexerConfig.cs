using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using RapidCore.Configuration;
using SapientFi.Kernel.Config;

namespace SapientFi.ServiceHost.Config;

public class CosmosIndexerConfig : IEnabledServiceRolesConfig, IDbConfig, IGatewayPoolsConfig, ICorsConfig, IFeatureConfig, IMessageTransportConfig
{
    private readonly IConfiguration _config;

    public CosmosIndexerConfig(IConfiguration config)
    {
        _config = config;
    }

    public List<string> EnabledRoles => _config.GetFromCommaSeparatedList("SAPIENTFI_SERVICE_ROLES_ENABLED", new List<string>
    {
        ServiceRoles.API,
        ServiceRoles.BACKGROUND_WORKER,
    });

    bool IEnabledServiceRolesConfig.IsRoleEnabled(string role)
    {
        return EnabledRoles.Contains(role);
    }

    string IDbConfig.ConnectionString => _config.Get(
        "SAPIENTFI_DB_CONNECTION_STRING",
        "User ID=sapientfi_indexer_user;Password=sapientfi_indexer_user_pass;Host=localhost;Port=35432;Database=sapientfi_indexer;Pooling=true;Minimum Pool Size=10;Maximum Pool Size=100;"
    );

    bool IDbConfig.RunMigrationsDuringBoot => _config.Get(
        "SAPIENTFI_DB_RUN_MIGRATIONS_ON_BOOT",
        true
    );

    int IGatewayPoolsConfig.NumberOfElementsInDepositsPrWallet => _config.Get(
        "SAPIENTFI_NR_ELEMENTS_DEPOSITS_PR_WALLET",
        11
    );

    List<string> ICorsConfig.AllowedOrigins => _config.GetFromCommaSeparatedList("SAPIENTFI_API_ALLOWED_CORS_ORIGINS", new List<string>
    {
        "http://localhost:3000",
    });

    bool IFeatureConfig.TriggerGatewayPoolFullResync => _config.Get("SAPIENTFI_TRIGGER_GATEWAY_POOL_FULL_RESYNC", false);
    
    bool IFeatureConfig.TriggerMineStakingFullResync => _config.Get("SAPIENTFI_TRIGGER_MINE_STAKING_FULL_RESYNC", false);
    bool IFeatureConfig.TriggerMineBuyBackFullResync  => _config.Get("SAPIENTFI_TRIGGER_MINE_BUYBACK_FULL_RESYNC", false);
    
    string IMessageTransportConfig.TransportUri => _config.Get("SAPIENTFI_MESSAGE_TRANSPORT_URI", "amqp://guest:guest@localhost:5672");
}