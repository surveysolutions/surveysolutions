using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Infrastructure.Native.Storage.Postgre.DbMigrations
{
    internal static class DbMigrationsRunner
    {
        public class MigrationOptions : IMigrationProcessorOptions
        {
            public bool PreviewOnly { get; set; }
            public string ProviderSwitches { get; set; }
            public int Timeout { get; set; }
        }

        public static void MigrateToLatest(string connectionString, DbUpgradeSettings dbUpgradeSettings)
        {
            // var announcer = new NullAnnouncer();
            var logger = ServiceLocator.Current.GetInstance<ILoggerProvider>().GetForType(typeof(DbMigrationsRunner));
            var announcer = new TextWriterAnnouncer(s => logger.Info(s)); 

            var migrationContext = new RunnerContext(announcer)
            {
                Namespace = dbUpgradeSettings.MigrationsNamespace
            };

            var options = new MigrationOptions
            {
                PreviewOnly = false,
            };
            var factory = new FluentMigrator.Runner.Processors.Postgres.PostgresProcessorFactory();

            using (var processor = factory.Create(connectionString, announcer, options))
            {
                var runner = new MigrationRunner(dbUpgradeSettings.MigrationsAssembly, migrationContext, processor);
                runner.MigrateUp();
            }
        }
    }
}