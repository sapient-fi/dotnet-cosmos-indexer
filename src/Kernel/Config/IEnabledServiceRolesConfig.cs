namespace Pylonboard.Kernel.Config;

public interface IEnabledServiceRolesConfig
{
    public List<string> EnabledRoles { get; }

    public bool IsRoleEnabled(string role);
}