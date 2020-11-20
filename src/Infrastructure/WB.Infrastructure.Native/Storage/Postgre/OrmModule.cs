using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NHibernate;
using NLog;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Exceptions;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Resources;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

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
                var loggerProvider = serviceLocator.GetInstance<ILoggerProvider>();
                
                status.Message = Modules.InitializingDb;
                DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString,
                    this.connectionSettings.PlainStorageSchemaName);

                status.Message = Modules.MigrateDb;

                await using var migrationLock = new MigrationLock(this.connectionSettings.ConnectionString);

                void MigrateReadside()
                {
                    if (this.connectionSettings.ReadSideUpgradeSettings != null)
                    {
                        status.Message = Modules.InitializingDb;
                        DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString,
                            this.connectionSettings.ReadSideSchemaName);

                        status.Message = Modules.MigrateDb;
                        DbMigrationsRunner.MigrateToLatest(this.connectionSettings.ConnectionString,
                            this.connectionSettings.ReadSideSchemaName,
                            this.connectionSettings.ReadSideUpgradeSettings,
                            loggerProvider);

                        status.ClearMessage();
                    }
                }

                void MigratePlainstore()
                {
                    DbMigrationsRunner.MigrateToLatest(this.connectionSettings.ConnectionString,
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

                if (this.connectionSettings.LogsUpgradeSettings != null)
                {
                    status.Message = Modules.InitializingDb;
                    DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString,
                        this.connectionSettings.LogsSchemaName);

                    status.Message = Modules.MigrateDb;
                    DbMigrationsRunner.MigrateToLatest(this.connectionSettings.ConnectionString,
                        this.connectionSettings.LogsSchemaName,
                        this.connectionSettings.LogsUpgradeSettings,
                        loggerProvider);

                    status.ClearMessage();
                }

                if (this.connectionSettings.UsersUpgradeSettings != null)
                {
                    status.Message = Modules.InitializingDb;
                    DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString,
                        this.connectionSettings.UsersSchemaName);

                    status.Message = Modules.MigrateDb;
                    DbMigrationsRunner.MigrateToLatest(this.connectionSettings.ConnectionString,
                        this.connectionSettings.UsersSchemaName,
                        this.connectionSettings.UsersUpgradeSettings,
                        loggerProvider);

                    status.ClearMessage();
                }
            }
            catch (Exception exc)
            {
                status.Error(Modules.ErrorDuringRunningMigrations, exc);

                LogManager.GetLogger(typeof(OrmModule).FullName).Fatal(exc, "Error during db initialization.");
                throw exc.AsInitializationException(connectionSettings.ConnectionString);
            }
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindToConstant(() => this.connectionSettings);
            registry.BindToMethodInSingletonScope<ISessionFactory>(context => new HqSessionFactoryFactory(this.connectionSettings).BuildSessionFactory());
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
            registry.Bind(typeof(IPlainKeyValueStorage<>), typeof(PostgresPlainKeyValueStorage<>));
            registry.BindAsSingleton(typeof(IEntitySerializer<>), typeof(EntitySerializer<>));
        }

    }

    public class UnitOfWorkConnectionSettings
    {
        public UnitOfWorkConnectionSettings()
        {
            this.ReadSideSchemaName = "readside";
            this.PlainStorageSchemaName = "plainstore";
        }

        public string ConnectionString { get; set; }

        public string PlainStorageSchemaName { get; set; }
        public string ReadSideSchemaName { get; set; }

        public IList<Assembly> PlainMappingAssemblies { get; set; }
        public IList<Assembly> ReadSideMappingAssemblies { get; set; }
        public DbUpgradeSettings ReadSideUpgradeSettings { get; set; }
        public DbUpgradeSettings PlainStoreUpgradeSettings { get; set; }
        public DbUpgradeSettings LogsUpgradeSettings { get; set; }
        public string LogsSchemaName => "logs";
        public DbUpgradeSettings UsersUpgradeSettings { get; set; }
        public string UsersSchemaName => "users";
    }
}
