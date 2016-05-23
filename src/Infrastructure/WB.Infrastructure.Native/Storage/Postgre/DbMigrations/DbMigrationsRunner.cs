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

        public static void MigrateToLatest(string connectionString, Assembly migrationsAssembly)
        {
            // var announcer = new NullAnnouncer();
            var logger = ServiceLocator.Current.GetInstance<ILoggerProvider>().GetForType(typeof(DbMigrationsRunner));
            var announcer = new TextWriterAnnouncer(s => logger.Info(s)); 

            var migrationContext = new RunnerContext(announcer);

            var options = new MigrationOptions { PreviewOnly = false, Timeout = 60 };
            var factory = new FluentMigrator.Runner.Processors.Postgres.PostgresProcessorFactory();

            using (var processor = factory.Create(connectionString, announcer, options))
            {
                var runner = new MigrationRunner(migrationsAssembly, migrationContext, processor);
                runner.MigrateUp(true);
            }
        }
    }
}