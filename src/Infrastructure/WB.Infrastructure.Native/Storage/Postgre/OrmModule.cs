using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Dapper;
using Humanizer;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NLog;
using Npgsql;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Resources;
using WB.Core.Infrastructure.Services;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;
using WB.Infrastructure.Native.Workspaces;

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
                    this.connectionSettings.PrimaryWorkspaceSchemaName);

                await using var migrationLock = new MigrationLock(this.connectionSettings.ConnectionString);

                status.Message = Modules.MigrateDb;

                var hasPlainstoreMigrations = DatabaseManagement.MigratedToWorkspaces(
                    this.connectionSettings.PlainStorageSchemaName, this.connectionSettings.ConnectionString);

                var hasWorkspacesMigrations = DatabaseManagement.MigratedToWorkspaces(
                    this.connectionSettings.PrimaryWorkspaceSchemaName, this.connectionSettings.ConnectionString);

                if (hasPlainstoreMigrations && !hasWorkspacesMigrations)
                {
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
                        DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString,
                            this.connectionSettings.PlainStorageSchemaName);
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

                    status.Message = Modules.InitializingDb;
                    DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString,
                        this.connectionSettings.EventsSchemaName);

                    status.Message = Modules.MigrateDb;
                    DbMigrationsRunner.MigrateToLatest(this.connectionSettings.ConnectionString,
                        this.connectionSettings.EventsSchemaName,
                        this.connectionSettings.EventStoreUpgradeSettings,
                        loggerProvider);

                    status.ClearMessage();
                }

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

                if (hasPlainstoreMigrations && !hasWorkspacesMigrations && this.connectionSettings.MigrateToPrimaryWorkspace != null)
                {
                    status.Message = Modules.MigrateDb;
                    DbMigrationsRunner.MigrateToLatest(this.connectionSettings.ConnectionString,
                        this.connectionSettings.PrimaryWorkspaceSchemaName,
                        this.connectionSettings.MigrateToPrimaryWorkspace,
                        loggerProvider);

                    status.ClearMessage();
                }

                if (this.connectionSettings.WorkspacesMigrationSettings != null)
                {
                    status.Message = Modules.InitializingDb;
                    DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString,
                        this.connectionSettings.WorkspacesSchemaName);

                    DbMigrationsRunner.MigrateToLatest(this.connectionSettings.ConnectionString,
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

                LogManager.GetLogger(typeof(OrmModule).FullName).Fatal(exc, "Error during db initialization.");
                throw exc.AsInitializationException(connectionSettings.ConnectionString);
            }
        }

        private async Task MigrateWorkspacesAsync(UnderConstructionInfo status, ILoggerProvider loggerProvider)
        {
            await using var connection = new NpgsqlConnection(this.connectionSettings.ConnectionString);
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
            registry.BindToMethod<ISessionFactory>(SessionFactoryBinder, externallyOwned: true);
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

        private ISessionFactory SessionFactoryBinder(IModuleContext context)
        {
            var workspace = context.Resolve<IWorkspaceContextAccessor>().CurrentWorkspace();

            return sessionFactories.GetOrAdd(workspace?.Name ?? WorkspaceConstants.SchemaName,
                space => new Lazy<ISessionFactory>(() => BuildSessionFactory(workspace?.SchemaName ?? WorkspaceConstants.SchemaName))).Value;
        }

        private static readonly ConcurrentDictionary<string, Lazy<ISessionFactory>> sessionFactories
            = new ConcurrentDictionary<string, Lazy<ISessionFactory>>();

        private ISessionFactory BuildSessionFactory(string workspaceSchema)
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                var connectionStringBuilder = new NpgsqlConnectionStringBuilder(this.connectionSettings.ConnectionString)
                {
                    SearchPath = workspaceSchema
                };

                var workspaceConnectionString = connectionStringBuilder.ToString();

                db.ConnectionString = workspaceConnectionString;
                db.Dialect<PostgreSQL91Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.Keywords;
            });

            var maps = this.GetWorkspaceMappings();
            var usersMaps = this.GetUsersMappings();

            cfg.AddDeserializedMapping(maps, "maps");
            cfg.AddDeserializedMapping(usersMaps, "users");
            cfg.SetProperty(NHibernate.Cfg.Environment.WrapResultSets, "true");

            // File.WriteAllText(@"D:\Temp\Mapping.xml" , Serialize(maps)); // Can be used to check mappings

            cfg.SessionFactory().GenerateStatistics();

            var sessionFactory = cfg.BuildSessionFactory();

            return sessionFactory;
        }

        private HbmMapping GetUsersMappings()
        {
            var mapper = new ModelMapper();
            var readSideMappingTypes = this.connectionSettings.ReadSideMappingAssemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => x.GetCustomAttribute<UsersAttribute>() != null &&
                            x.IsSubclassOfRawGeneric(typeof(ClassMapping<>)));

            mapper.AfterMapProperty += (inspector, member, customizer) =>
            {
                var propertyInfo = (PropertyInfo)member.LocalMember;

                customizer.Column('"' + propertyInfo.Name + '"');
            };

            mapper.AddMappings(readSideMappingTypes);

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }

        /// <summary>
        /// Generates XML string from <see cref="NHibernate"/> mappings. Used just to verify what was generated by ConfOrm to make sure everything is correct.
        /// </summary>
        protected static string Serialize(HbmMapping hbmElement)
        {
            var setting = new XmlWriterSettings { Indent = true };
            var serializer = new XmlSerializer(typeof(HbmMapping));
            using (var memStream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(memStream, setting))
                {
                    serializer.Serialize(xmlWriter, hbmElement);
                    memStream.Flush();
                    byte[] streamContents = memStream.ToArray();

                    string result = Encoding.UTF8.GetString(streamContents);
                    return result;
                }
            }
        }

        private HbmMapping GetWorkspaceMappings()
        {
            var mapper = new ModelMapper();
            var readSideMappingTypes = this.connectionSettings.ReadSideMappingAssemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => x.GetCustomAttribute<UsersAttribute>() == null &&
                            x.IsSubclassOfRawGeneric(typeof(ClassMapping<>)));

            mapper.AddMappings(readSideMappingTypes);
            CustomizeMappings(mapper);

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }

        private static void CustomizeMappings(ModelMapper mapper)
        {
            mapper.BeforeMapProperty += (inspector, member, customizer) =>
            {
                var propertyInfo = (PropertyInfo)member.LocalMember;
                if (propertyInfo.PropertyType == typeof(string))
                {
                    customizer.Type(NHibernateUtil.StringClob);
                }

                // Someone decided that its a good idea to break existing conventions for column namings for this 2 tables
                // But I don't want to rename them since portal and powershell scripts are reading them
                if (member.LocalMember.DeclaringType.Name == "DeviceSyncInfo" ||
                    member.LocalMember.DeclaringType.Name == "SyncStatistics")
                {
                    customizer.Column('"' + propertyInfo.Name + '"');
                }
            };

            mapper.BeforeMapClass += (inspector, type, customizer) =>
            {
                var tableName = type.Name.Pluralize();
                customizer.Table(tableName);
            };
        }
    }
}
