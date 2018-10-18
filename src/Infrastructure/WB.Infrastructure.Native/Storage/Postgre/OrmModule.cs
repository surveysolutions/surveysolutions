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
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Resources;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class OrmModule : IModule
    {
        private readonly UnitOfWorkConnectionSettings connectionSettings;

        public OrmModule(UnitOfWorkConnectionSettings connectionSettings) 
        {
            this.connectionSettings = connectionSettings;
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            try
            {
                status.Message = Modules.InitializingDb;
                DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString,
                    this.connectionSettings.PlainStorageSchemaName);

                status.Message = Modules.MigrateDb;
                DbMigrationsRunner.MigrateToLatest(this.connectionSettings.ConnectionString,
                    this.connectionSettings.PlainStorageSchemaName,
                    this.connectionSettings.PlainStoreUpgradeSettings);
            }
            catch (Exception exc)
            {
                LogManager.GetLogger(nameof(OrmModule)).Fatal(exc, "Error during db initialization.");
                throw;
            }

            if (this.connectionSettings.ReadSideUpgradeSettings != null)
            {
                try
                {
                    status.Message = Modules.InitializingDb;
                    DatabaseManagement.InitDatabase(this.connectionSettings.ConnectionString,
                        this.connectionSettings.ReadSideSchemaName);

                    status.Message = Modules.MigrateDb;
                    DbMigrationsRunner.MigrateToLatest(this.connectionSettings.ConnectionString,
                        this.connectionSettings.ReadSideSchemaName, this.connectionSettings.ReadSideUpgradeSettings);
                }
                catch (Exception exc)
                {
                    LogManager.GetLogger(nameof(OrmModule)).Fatal(exc, "Error during db initialization.");
                    throw new InitializationException(Subsystem.Database, null, exc);
                }
            }

            return Task.CompletedTask;
        }

        public void Load(IIocRegistry registry)
        {
            registry.BindToConstant(() => this.connectionSettings);
            registry.BindToMethodInSingletonScope<ISessionFactory>(context => this.BuildSessionFactory());
            registry.BindInPerUnitOfWorkOrPerRequestScope<IUnitOfWork, UnitOfWork>();

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

            cfg.AddDeserializedMapping(maps, "");
            cfg.AddDeserializedMapping(plainMaps, "");
            cfg.SetProperty(NHibernate.Cfg.Environment.WrapResultSets, "true");

            //File.WriteAllText(@"D:\Temp\Mapping.xml" , Serialize(maps)); // Can be used to check mappings

            cfg.SessionFactory().GenerateStatistics();

            var sessionFactory = cfg.BuildSessionFactory();

            MetricsRegistry.Instance.RegisterOnDemandCollectors(
                new NHibernateStatsCollector("", sessionFactory)
            );

            return sessionFactory;
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

        private HbmMapping GetReadSideMappings()
        {
            var mapper = new ModelMapper();
            var readSideMappingTypes = this.connectionSettings.ReadSideMappingAssemblies.SelectMany(x => x.GetExportedTypes())
                .Where(x => x.GetCustomAttribute<PlainStorageAttribute>() == null && 
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
    }
}
