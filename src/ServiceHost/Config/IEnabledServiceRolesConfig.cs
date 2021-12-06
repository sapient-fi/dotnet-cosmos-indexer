namespace Pylonboard.ServiceHost.Config;

public interface IEnabledServiceRolesConfig
{
    public List<string> EnabledRoles { get; }

    public bool IsRoleEnabled(string role);
}