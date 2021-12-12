using RapidCore.Configuration;

namespace Pylonboard.ServiceHost.Config;

public class PylonboardConfig : IEnabledServiceRolesConfig, IDbConfig
{
    private readonly IConfiguration _config;

    public PylonboardConfig(IConfiguration config)
    {
        _config = config;
    }

    public List<string> EnabledRoles => _config.GetFromCommaSeparatedList<string>("PYLONBOARD_SERVICE_ROLES_ENABLED", new List<string>
    {
        ServiceRoles.API,
        ServiceRoles.BACKGROUND_WORKER,
    });

    public bool IsRoleEnabled(string role)
    {
        return EnabledRoles.Contains(role);
    }

    public string ConnectionString => _config.Get(
        "PYLONBOARD_DB_CONNECTION_STRING",
        "User ID=pylonboard_user;Password=pylonboard_user_pass;Host=localhost;Port=35432;Database=pylonboard;Pooling=true;Minimum Pool Size=10;Maximum Pool Size=100;"
    );

    public bool DisableMigrationsDuringBoot => _config.Get(
        "PYLONBOARD_DB_DISABLE_MIGRATIONS_ON_BOOT",
        false
    );
}