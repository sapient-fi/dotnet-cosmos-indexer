using RapidCore.Configuration;
using Sapient.Kernel.Config;

namespace Sapient.ServiceHost.Config;

public class PylonboardConfig : IEnabledServiceRolesConfig, IDbConfig, IGatewayPoolsConfig, ICorsConfig, IFeatureConfig, IMessageTransportConfig
{
    private readonly IConfiguration _config;

    public PylonboardConfig(IConfiguration config)
    {
        _config = config;
    }

    public List<string> EnabledRoles => _config.GetFromCommaSeparatedList<string>("SAPIENT_SERVICE_ROLES_ENABLED", new List<string>
    {
        ServiceRoles.API,
        ServiceRoles.BACKGROUND_WORKER,
    });

    bool IEnabledServiceRolesConfig.IsRoleEnabled(string role)
    {
        return EnabledRoles.Contains(role);
    }

    string IDbConfig.ConnectionString => _config.Get(
        "SAPIENT_DB_CONNECTION_STRING",
        "User ID=sapient_user;Password=sapient_user_pass;Host=localhost;Port=35432;Database=pylonboard;Pooling=true;Minimum Pool Size=10;Maximum Pool Size=100;"
    );

    bool IDbConfig.DisableMigrationsDuringBoot => _config.Get(
        "SAPIENT_DB_DISABLE_MIGRATIONS_ON_BOOT",
        false
    );

    int IGatewayPoolsConfig.NumberOfElementsInDepositsPrWallet => _config.Get(
        "SAPIENT_NR_ELEMENTS_DEPOSITS_PR_WALLET",
        11
    );

    List<string> ICorsConfig.AllowedOrigins => _config.GetFromCommaSeparatedList<string>("SAPIENT_API_ALLOWED_CORS_ORIGINS", new List<string>
    {
        "http://localhost:3000",
    });

    bool IFeatureConfig.TriggerGatewayPoolFullResync => _config.Get("SAPIENT_TRIGGER_GATEWAY_POOL_FULL_RESYNC", false);
    
    bool IFeatureConfig.TriggerMineStakingFullResync => _config.Get("SAPIENT_TRIGGER_MINE_STAKING_FULL_RESYNC", false);
    bool IFeatureConfig.TriggerMineBuyBackFullResync  => _config.Get("SAPIENT_TRIGGER_MINE_BUYBACK_FULL_RESYNC", false);
    
    string IMessageTransportConfig.TransportUri => _config.Get("SAPIENT_MESSAGE_TRANSPORT_URI", "amqp://guest:guest@localhost:5672");
}