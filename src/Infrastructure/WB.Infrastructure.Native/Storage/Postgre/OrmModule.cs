using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using NHibernate;
using Npgsql;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Resources;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Utils;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class OrmModule : IModule, IInitModule
    {
        private readonly UnitOfWorkConnectionSettings connectionSettings;

        public OrmModule(UnitOfWorkConnectionSettings connectionSettings)
        {
            this.connectionSettings = connectionSettings;
        }

        public async Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            try
            {
                var connectionStringBuilder = new NpgsqlConnectionStringBuilder(this.connectionSettings.ConnectionString);
                connectionStringBuilder.Pooling = false;
                connectionStringBuilder.SetApplicationPostfix("orm");
                var connectionString = connectionStringBuilder.ConnectionString;

                var loggerProvider = serviceLocator.GetInstance<ILoggerProvider>();
                status.Message = Modules.InitializingDb;

                DatabaseManagement.InitDatabase(connectionString,
                    this.connectionSettings.PrimaryWorkspaceSchemaName);

                await using var migrationLock = new MigrationLock(connectionString);

                status.Message = Modules.MigrateDb;

                var hasPlainstoreMigrations = DatabaseManagement.MigratedToWorkspaces(
                    this.connectionSettings.PlainStorageSchemaName, connectionString);

                var hasWorkspacesMigrations = DatabaseManagement.MigratedToWorkspaces(
                    this.connectionSettings.PrimaryWorkspaceSchemaName, connectionString);

                if (hasPlainstoreMigrations && !hasWorkspacesMigrations)
                {
                    void MigrateReadside()
                    {
                        if (this.connectionSettings.ReadSideUpgradeSettings != null)
                        {
                            status.Message = Modules.InitializingDb;
                            DatabaseManagement.InitDatabase(connectionString,
                                this.connectionSettings.ReadSideSchemaName);

                            status.Message = Modules.MigrateDb;
                            DbMigrationsRunner.MigrateToLatest(connectionString,
                                this.connectionSettings.ReadSideSchemaName,
                                this.connectionSettings.ReadSideUpgradeSettings,
                                loggerProvider);

                            status.ClearMessage();
                        }
                    }

                    void MigratePlainstore()
                    {
                        DatabaseManagement.InitDatabase(connectionString,
                            this.connectionSettings.PlainStorageSchemaName);
                        DbMigrationsRunner.MigrateToLatest(connectionString,
                            this.connectionSettings.PlainStorageSchemaName,
                            this.connectionSettings.PlainStoreUpgradeSettings,
                            loggerProvider);

                        status.ClearMessage();
                    }

                    try
                    {
                        MigratePlainstore();
                    }
                    catch
                    {
                        MigrateReadside();
                        MigratePlainstore();
                    }

                    MigrateReadside();

                    status.Message = Modules.InitializingDb;
                    DatabaseManagement.InitDatabase(connectionString,
                        this.connectionSettings.EventsSchemaName);

                    status.Message = Modules.MigrateDb;
                    DbMigrationsRunner.MigrateToLatest(connectionString,
                        this.connectionSettings.EventsSchemaName,
                        this.connectionSettings.EventStoreUpgradeSettings,
                        loggerProvider);

                    status.ClearMessage();
                }

                if (this.connectionSettings.LogsUpgradeSettings != null)
                {
                    status.Message = Modules.InitializingDb;
                    DatabaseManagement.InitDatabase(connectionString,
                        this.connectionSettings.LogsSchemaName);

                    status.Message = Modules.MigrateDb;
                    DbMigrationsRunner.MigrateToLatest(connectionString,
                        this.connectionSettings.LogsSchemaName,
                        this.connectionSettings.LogsUpgradeSettings,
                        loggerProvider);

                    status.ClearMessage();
                }

                if (this.connectionSettings.UsersUpgradeSettings != null)
                {
                    status.Message = Modules.InitializingDb;
                    DatabaseManagement.InitDatabase(connectionString,
                        this.connectionSettings.UsersSchemaName);

                    status.Message = Modules.MigrateDb;
                    DbMigrationsRunner.MigrateToLatest(connectionString,
                        this.connectionSettings.UsersSchemaName,
                        this.connectionSettings.UsersUpgradeSettings,
                        loggerProvider);

                    status.ClearMessage();
                }

                if (hasPlainstoreMigrations && !hasWorkspacesMigrations && this.connectionSettings.MigrateToPrimaryWorkspace != null)
                {
                    status.Message = Modules.MigrateDb;
                    DbMigrationsRunner.MigrateToLatest(connectionString,
                        this.connectionSettings.PrimaryWorkspaceSchemaName,
                        this.connectionSettings.MigrateToPrimaryWorkspace,
                        loggerProvider);

                    status.ClearMessage();
                }

                if (this.connectionSettings.WorkspacesMigrationSettings != null)
                {
                    status.Message = Modules.InitializingDb;
                    DatabaseManagement.InitDatabase(connectionString,
                        this.connectionSettings.WorkspacesSchemaName);

                    DbMigrationsRunner.MigrateToLatest(connectionString,
                        this.connectionSettings.WorkspacesSchemaName,
                        this.connectionSettings.WorkspacesMigrationSettings,
                        loggerProvider);

                    status.ClearMessage();
                }

                await MigrateWorkspacesAsync(status, loggerProvider);
            }
            catch (Exception exc)
            {
                status.Error(Modules.ErrorDuringRunningMigrations, exc);
                serviceLocator.GetInstance<ILogger<OrmModule>>().LogCritical(exc, "Error during db initialization.");
                throw exc.AsInitializationException(connectionSettings.ConnectionString);
            }
        }

        private async Task MigrateWorkspacesAsync(UnderConstructionInfo status, ILoggerProvider loggerProvider)
        {
            var connectionStringBuilder = new NpgsqlConnectionStringBuilder(this.connectionSettings.ConnectionString);
            connectionStringBuilder.Pooling = false;
            connectionStringBuilder.SetApplicationPostfix("migrate_workspaces");
            await using var connection = new NpgsqlConnection(connectionStringBuilder.ConnectionString);
            
            IEnumerable<string> workspaces =
                await connection.QueryAsync<string>($"select name from {this.connectionSettings.WorkspacesSchemaName}.workspaces");

            foreach (var workspace in workspaces)
            {
                status.Message = Modules.InitializingDb;
                var targetSchema = "ws_" + workspace;

                DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString, targetSchema);

                DbMigrationsRunner.MigrateToLatest(this.connectionSettings.ConnectionString,
                    targetSchema,
                    this.connectionSettings.SingleWorkspaceUpgradeSettings,
                    loggerProvider);

                status.ClearMessage();
            }
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindToConstant(() => this.connectionSettings);
            
            var sessionFactory = new HqSessionFactoryFactory(this.connectionSettings);
            registry.BindToMethod<ISessionFactory>(c => sessionFactory.SessionFactoryBinder(c), 
                externallyOwned: true);

            registry.BindInPerLifetimeScope<IUnitOfWork, UnitOfWork>();

            registry.Bind(typeof(IQueryableReadSideRepositoryReader<>), typeof(PostgreReadSideStorage<>));
            registry.Bind(typeof(IQueryableReadSideRepositoryReader<,>), typeof(PostgreReadSideStorage<,>));

            registry.Bind(typeof(IReadSideRepositoryReader<>), typeof(PostgreReadSideStorage<>));
            registry.Bind(typeof(IReadSideRepositoryReader<,>), typeof(PostgreReadSideStorage<,>));

            registry.Bind(typeof(INativeReadSideStorage<>), typeof(PostgreReadSideStorage<>));
            registry.Bind(typeof(INativeReadSideStorage<,>), typeof(PostgreReadSideStorage<,>));

            registry.Bind(typeof(PostgreReadSideStorage<>), typeof(PostgreReadSideStorage<>));
            registry.Bind(typeof(PostgreReadSideStorage<,>), typeof(PostgreReadSideStorage<,>));

            registry.Bind(typeof(IReadSideRepositoryWriter<>), typeof(PostgreReadSideStorage<>));
            registry.Bind(typeof(IReadSideRepositoryWriter<,>), typeof(PostgreReadSideStorage<,>));

            registry.Bind(typeof(IPlainStorageAccessor<>), typeof(PostgresPlainStorageRepository<>));
            registry.Bind(typeof(IPlainKeyValueStorage<>), typeof(PostgresKeyValueStorageWithCache<>));
            registry.BindAsSingleton(typeof(IEntitySerializer<>), typeof(EntitySerializer<>));
        }

    }
}
