using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Humanizer;
using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NLog;
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
            registry.BindToMethodInSingletonScope<ISessionFactory>(context => this.BuildSessionFactory());
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

        private ISessionFactory BuildSessionFactory()
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = this.connectionSettings.ConnectionString;
                db.Dialect<PostgreSQL91Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.Keywords;
            });

            var maps = this.GetReadSideMappings();
            var plainMaps = this.GetPlainMappings();
            var usersMaps = this.GetUsersMappings();

            cfg.AddDeserializedMapping(maps, "read");
            cfg.AddDeserializedMapping(plainMaps, "plain");
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
                var propertyInfo = (PropertyInfo) member.LocalMember;

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
            var setting = new XmlWriterSettings {Indent = true};
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

        private HbmMapping GetReadSideMappings()
        {
            var mapper = new ModelMapper();
            var readSideMappingTypes = this.connectionSettings.ReadSideMappingAssemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => x.GetCustomAttribute<PlainStorageAttribute>() == null &&
                            x.GetCustomAttribute<UsersAttribute>() == null &&
                            x.IsSubclassOfRawGeneric(typeof(ClassMapping<>)));

            mapper.AddMappings(readSideMappingTypes);
            var schemaName = this.connectionSettings.ReadSideSchemaName;

            CustomizeMappings(mapper, schemaName);

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }

        private HbmMapping GetPlainMappings()
        {
            var mapper = new ModelMapper();
            var plainStoreMappingTypes = this.connectionSettings.PlainMappingAssemblies
                .SelectMany(x => x.GetExportedTypes())
                .Where(x => x.GetCustomAttribute<PlainStorageAttribute>() != null &&
                            x.IsSubclassOfRawGeneric(typeof(ClassMapping<>)));
            mapper.AddMappings(plainStoreMappingTypes);
            CustomizeMappings(mapper, this.connectionSettings.PlainStorageSchemaName);

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }

        private static void CustomizeMappings(ModelMapper mapper, string schemaName)
        {
            mapper.BeforeMapProperty += (inspector, member, customizer) =>
            {
                var propertyInfo = (PropertyInfo) member.LocalMember;
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
                customizer.Schema(schemaName);
            };

            mapper.BeforeMapSet += (inspector, member, customizer) => customizer.Schema(schemaName);
            mapper.BeforeMapBag += (inspector, member, customizer) => customizer.Schema(schemaName);
            mapper.BeforeMapList += (inspector, member, customizer) => customizer.Schema(schemaName);
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
