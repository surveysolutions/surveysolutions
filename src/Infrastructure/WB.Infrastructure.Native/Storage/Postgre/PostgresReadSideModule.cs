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
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Resources;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class PostgresReadSideModule : PostgresModuleWithCache
    {
        public const string ReadSideSessionFactoryName = "ReadSideSessionFactory";
        public static readonly string ReadSideSchemaName = "readside";
        internal const string SessionProviderName = "ReadSideProvider";
        private readonly string connectionString;
        private readonly string schemaName;
        private readonly DbUpgradeSettings dbUpgradeSettings;
        private readonly IEnumerable<Assembly> mappingAssemblies;
        private readonly bool runInitAndMigrations;


        public PostgresReadSideModule(string connectionString, 
            string schemaName,
            DbUpgradeSettings dbUpgradeSettings,
            ReadSideCacheSettings cacheSettings,
            IEnumerable<Assembly> mappingAssemblies,
            bool runInitAndMigrations = true)
            : base(cacheSettings)
        {
            this.connectionString = connectionString;
            this.schemaName = schemaName;
            this.dbUpgradeSettings = dbUpgradeSettings;
            this.mappingAssemblies = mappingAssemblies;
            this.runInitAndMigrations = runInitAndMigrations;
        }

        protected override IReadSideStorage<TEntity, TKey> GetPostgresReadSideStorage<TEntity, TKey>(IModuleContext context)
            => (IReadSideStorage<TEntity, TKey>) context.GetServiceWithGenericType(typeof(PostgreReadSideStorage<,>), typeof(TEntity), typeof(TKey));

        protected override IReadSideStorage<TEntity> GetPostgresReadSideStorage<TEntity>(IModuleContext context)
            => (IReadSideStorage<TEntity>) context.GetServiceWithGenericType(typeof(PostgreReadSideStorage<>), typeof(TEntity));

        public override void Load(IIocRegistry registry)
        {
            base.Load(registry);

            registry.BindToConstant<PostgreConnectionSettings>(() => new PostgreConnectionSettings
            {
                ConnectionString = this.connectionString,
                SchemaName = ReadSideSchemaName
            });

            registry.BindWithConstructorArgument<IPostgresReadSideBootstraper, PostgresReadSideBootstraper>("dbUpgradeSettings", this.dbUpgradeSettings);

            registry.BindAsSingleton(typeof(IQueryableReadSideRepositoryReader<>), typeof(PostgreReadSideStorage<>));
            registry.BindAsSingleton(typeof(IQueryableReadSideRepositoryReader<,>), typeof(PostgreReadSideStorage<,>));

            registry.BindAsSingleton(typeof(IReadSideRepositoryReader<>), typeof(PostgreReadSideStorage<>));
            registry.BindAsSingleton(typeof(IReadSideRepositoryReader<,>), typeof(PostgreReadSideStorage<,>)); 

            registry.BindAsSingleton(typeof(INativeReadSideStorage<>), typeof(PostgreReadSideStorage<>));
            registry.BindAsSingleton(typeof(INativeReadSideStorage<,>), typeof(PostgreReadSideStorage<,>));

            registry.BindAsSingleton(typeof(PostgreReadSideStorage<>), typeof(PostgreReadSideStorage<>));
            registry.BindAsSingleton(typeof(PostgreReadSideStorage<,>), typeof(PostgreReadSideStorage<,>));

            registry.BindToMethodInSingletonScope(typeof(IReadSideRepositoryWriter<>), this.GetReadSideStorageWrappedWithCache);
            registry.BindToMethodInSingletonScope(typeof(IReadSideRepositoryWriter<,>), this.GetGenericReadSideStorageWrappedWithCache);

            registry.BindToMethodInSingletonScope<ISessionFactory>(kernel => this.BuildSessionFactory(), ReadSideSessionFactoryName);

            registry.BindInIsolatedThreadScopeOrRequestScopeOrThreadScope<CqrsPostgresTransactionManager>();
            registry.BindInIsolatedThreadScopeOrRequestScopeOrThreadScope<NoTransactionCqrsPostgresTransactionManager>();

            registry.BindToMethod<Func<CqrsPostgresTransactionManager>>(context => context.Get<CqrsPostgresTransactionManager>);
            registry.BindToMethod<Func<NoTransactionCqrsPostgresTransactionManager>>(context => () => context.Get<NoTransactionCqrsPostgresTransactionManager>());

            registry.BindToConstructorInSingletonScope<TransactionManagerProvider>(c => new TransactionManagerProvider(
                    c.Inject<Func<CqrsPostgresTransactionManager>>(),
                    c.Inject<Func<NoTransactionCqrsPostgresTransactionManager>>()));

            registry.BindToMethod<ISessionProvider>(context => context.Get<TransactionManagerProvider>(), SessionProviderName);
            registry.BindToMethod<ITransactionManager>(context => context.Get<CqrsPostgresTransactionManager>());

            registry.BindToMethod<ITransactionManagerProvider>(context => context.Get<TransactionManagerProvider>());
            registry.BindToMethod<ITransactionManagerProviderManager>(context => context.Get<TransactionManagerProvider>());
        }

        public override Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            if (runInitAndMigrations)
            {
                try
                {
                    status.Message = Modules.InitializingDb;
                    DatabaseManagement.InitDatabase(this.connectionString, this.schemaName);

                    status.Message = Modules.MigrateDb;
                    DbMigrationsRunner.MigrateToLatest(this.connectionString, this.schemaName, this.dbUpgradeSettings);
                }
                catch (Exception exc)
                {
                    LogManager.GetLogger("maigration", typeof(PostgresReadSideModule)).Fatal(exc, "Error during db initialization.");
                    throw new InitializationException(Subsystem.Database, null, exc);
                }
            }

            return base.Init(serviceLocator, status);
        }

        private object GetEntityIdentifierColumnName(IModuleContext context)
        {
            var entityType = context.GetGenericArgument();

            var sessionFactory = context.Get<ISessionFactory>(ReadSideSessionFactoryName);

            var persister = sessionFactory.GetClassMetadata(entityType);

            return persister?.IdentifierPropertyName;
        }

        private ISessionFactory BuildSessionFactory()
        {
            //File.WriteAllText(@"D:\Temp\Mapping.xml" ,Serialize(this.GetMappings())); // Can be used to check mappings
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = this.connectionString;
                db.Dialect<PostgreSQL91Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.Keywords;
            });
            
            cfg.SetProperty(NHibernate.Cfg.Environment.WrapResultSets, "true");
            cfg.AddDeserializedMapping(this.GetMappings(), "Main");

            cfg.SetProperty(NHibernate.Cfg.Environment.DefaultSchema, this.schemaName);
            cfg.SessionFactory().GenerateStatistics();

            var sessionFactory = cfg.BuildSessionFactory();
            
            MetricsRegistry.Instance.RegisterOnDemandCollectors(
                new NHibernateStatsCollector("readside", sessionFactory)
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

        private HbmMapping GetMappings()
        {
            var mapper = new ModelMapper();
            var mappingTypes = this.mappingAssemblies.SelectMany(x => x.GetExportedTypes())
                                                     .Where(x => x.GetCustomAttribute<PlainStorageAttribute>() == null && 
                                                                 x.IsSubclassOfRawGeneric(typeof(ClassMapping<>)));
            mapper.AddMappings(mappingTypes);
            mapper.BeforeMapProperty += (inspector, member, customizer) =>
            {
                var propertyInfo = (PropertyInfo) member.LocalMember;
                if (propertyInfo.PropertyType == typeof (string))
                {
                    customizer.Type(NHibernateUtil.StringClob);
                }
            };

            mapper.BeforeMapClass += (inspector, type, customizer) =>
            {
                var tableName = type.Name.Pluralize();
                customizer.Table(tableName);
                customizer.Schema(this.schemaName);
            };

            mapper.BeforeMapSet += (inspector, member, customizer) => customizer.Schema(this.schemaName);
            mapper.BeforeMapBag += (inspector, member, customizer) => customizer.Schema(this.schemaName);
            mapper.BeforeMapList += (inspector, member, customizer) => customizer.Schema(this.schemaName);

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }
    }
}
