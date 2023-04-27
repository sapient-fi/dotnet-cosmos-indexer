namespace SapientFi.Kernel.Config;

public interface IDbConfig
{
    public string ConnectionString { get; }

    public bool RunMigrationsDuringBoot { get; }
}