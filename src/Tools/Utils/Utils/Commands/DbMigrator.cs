using System;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.Implementation.Migrations;
using WB.UI.Headquarters.Migrations.PlainStore;
using WB.UI.Headquarters.Migrations.ReadSide;
using WB.UI.Headquarters.Migrations.Users;

namespace Utils.Commands
{
    public class DbMigrator
    {
        private readonly ILogger logger;

        public DbMigrator(ILogger logger)
        {
            this.logger = logger;
        }

        public int Run(string serverName, string connectionString)
        {
            logger.Info($"Migration of db {serverName}");

            var EventsSchemaName = "events";
            var PlainStoreSchemaName = "plainstore";
            var ReadSideSchemaName = "readside";
            var UsersSchemaName = "users";
            
            try
            {
                DatabaseManagement.InitDatabase(connectionString, EventsSchemaName);
                DatabaseManagement.InitDatabase(connectionString, PlainStoreSchemaName);
                DatabaseManagement.InitDatabase(connectionString, ReadSideSchemaName);
                DatabaseManagement.InitDatabase(connectionString, UsersSchemaName);

                MigrateToLatest(connectionString, EventsSchemaName, DbUpgradeSettings.FromFirstMigration<M001_AddEventSequenceIndex>());
                MigrateToLatest(connectionString, PlainStoreSchemaName, DbUpgradeSettings.FromFirstMigration<M001_Init>());
                MigrateToLatest(connectionString, ReadSideSchemaName, DbUpgradeSettings.FromFirstMigration<M001_InitDb>());
                MigrateToLatest(connectionString, UsersSchemaName, DbUpgradeSettings.FromFirstMigration<M001_AddUsersHqIdentityModel>());
            }
            catch (Exception exc)
            {
                logger.Fatal("Error during db initialization.", exc);
                Console.WriteLine(exc.ToString());
            }


            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"Finished at {DateTime.Now}");
            return 0;
        }

        public void MigrateToLatest(string connectionString, string schemaName, DbUpgradeSettings dbUpgradeSettings)
        {
            var announcer = new TextWriterAnnouncer(s =>
            {
                logger.Info(s);
                Console.WriteLine(s);
            })
            {
                ShowSql = true,
                ShowElapsedTime = true
            }; 

            var migrationContext = new RunnerContext(announcer)
            {
                Namespace = dbUpgradeSettings.MigrationsNamespace,
            };

            var options = new DbMigrationsRunner.MigrationOptions
            {
                PreviewOnly = false
            };

            var factory = new InSchemaPostgresProcessorFactory(schemaName);
            using (var processor = factory.Create(connectionString, announcer, options))
            {
                var runner = new MigrationRunner(dbUpgradeSettings.MigrationsAssembly, migrationContext, processor);
                runner.MigrateUp();
            }
        }
    }
}
