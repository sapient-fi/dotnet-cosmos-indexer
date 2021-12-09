using System.Reflection;
using Pylonboard.ServiceHost.Config;
using Pylonboard.ServiceHost.DAL;
using RapidCore.DependencyInjection;
using RapidCore.Migration;
using RapidCore.PostgreSql.Migration;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace Pylonboard.ServiceHost.ServiceCollectionExtensions;

public static class DbServiceCollectionExtensions
{
    public static IServiceCollection AddDbStack(this IServiceCollection services, IConfiguration configuration)
    {
        AddConfig(services, configuration);

        AddPostgresDbConnection(services);

        AddPostgresMigrationRunner(services, new[]
        {
            typeof(TerraMoneyServiceCollectionExtensions).Assembly,
        });

        return services;
    }

    /// <summary>
    /// register all the required things to connect to postgres
    /// </summary>
    /// <param name="services"></param>
    private static void AddPostgresDbConnection(IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory>(c =>
            new OrmLiteConnectionFactory(c.GetRequiredService<IDbConfig>().ConnectionString,
                PostgreSqlDialect.Provider)
        );
    }


    /// <summary>
    /// Register fully configured Postgres based <see cref="RapidCore.Migration.MigrationRunner"/>
    /// </summary>
    /// <param name="services">The services</param>
    /// <param name="assemblies">The main assemblies used for finding available migrations</param>
    public static IServiceCollection AddPostgresMigrationRunner(
        this IServiceCollection services,
        Assembly[] assemblies
    )
    {
        services.AddSingleton<PostgreSqlConnectionProvider>(c =>
        {
            var scoped = c.CreateScope();
            var pgFactory = scoped.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

            var commandTimeoutInSeconds = (int)Math.Ceiling(TimeSpan.FromMinutes(60).TotalSeconds);
            var ormliteConnection = pgFactory.OpenDbConnection();
            ormliteConnection.SetCommandTimeout(commandTimeoutInSeconds);

            var builder = new Npgsql.NpgsqlConnectionStringBuilder(ormliteConnection.ConnectionString)
            {
                CommandTimeout = commandTimeoutInSeconds
            };

            var npgsqlConnection = new Npgsql.NpgsqlConnection(builder.ConnectionString);
            npgsqlConnection.Open();

            var provider = new PostgreSqlConnectionProvider();

            // the default connection is used by RapidCore to
            // store migration meta data ... RapidCore uses Dapper
            // which is doing some weird casting under the hood
            // and fails if the connection cannot be cast to a DbConnection....
            // therefore we need the default connection to be a raw
            // NpgsqlConnection
            provider.Add("main", npgsqlConnection, true);

            // we also register a named connection which is the ormlite
            // given connection
            provider.Add("ormlite", ormliteConnection, false);


            return provider;
        });

        services.AddSingleton<IMigrationEnvironment>(c =>
        {
            var env = c.GetRequiredService<IHostEnvironment>();
            return new MigrationEnvironment(env.EnvironmentName.ToLowerInvariant());
        });

        services.AddSingleton<IMigrationContextFactory, PostgreSqlMigrationContextFactory>();
        services.AddSingleton<IMigrationFinder>(new ReflectionMigrationFinder(assemblies));
        services.AddSingleton<IMigrationStorage, PostgreSqlMigrationStorage>();
        services.AddSingleton<MigrationRunner, PylonboardMigrationRunner>();
        services.AddSingleton<IRapidContainerAdapter, ServiceProviderRapidContainerAdapter>();

        return services;
    }

    private static void AddConfig(IServiceCollection services, IConfiguration configuration)
    {
        var config = new PylonboardConfig(configuration);
        services.AddSingleton<IDbConfig>(config);
    }
}