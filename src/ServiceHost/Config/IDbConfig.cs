namespace Pylonboard.ServiceHost.Config
{
    public interface IDbConfig
    {
        public string ConnectionString { get; }

        public bool DisableMigrationsDuringBoot { get; }
    }
}