using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Humanizer;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NLog;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Monitoring;
using WB.Infrastructure.Native.Resources;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class PostgresPlainStorageModule : IModule
    {
        public const string SessionFactoryName = "PlainSessionFactory";
        private readonly PostgresPlainStorageSettings settings;

        public PostgresPlainStorageModule(PostgresPlainStorageSettings settings)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public void Load(IIocRegistry registry)
        {
            //registry.BindToConstant<PostgresPlainStorageSettings>(() => this.settings);
            
            //registry.BindToMethodInSingletonScope<ISessionFactory>(context => this.BuildSessionFactory(settings.SchemaName), SessionFactoryName);

            //registry.BindInIsolatedThreadScopeOrRequestScopeOrThreadScope<PlainPostgresTransactionManager>();
            //registry.BindInIsolatedThreadScopeOrRequestScopeOrThreadScope<NoTransactionPlainPostgresTransactionManager>();

            //registry.BindToMethod<Func<PlainPostgresTransactionManager>>(context => () => context.Get<PlainPostgresTransactionManager>());
            //registry.BindToMethod<Func<NoTransactionPlainPostgresTransactionManager>>(context => () => context.Get<NoTransactionPlainPostgresTransactionManager>());

            //registry
            //    .BindToConstructorInSingletonScope<PlainTransactionManagerProvider>(constructor => new PlainTransactionManagerProvider(
            //        constructor.Inject<Func<PlainPostgresTransactionManager>>(),
            //        constructor.Inject<Func<NoTransactionPlainPostgresTransactionManager>>()));

            //registry.BindToMethod<IPlainSessionProvider>(context => context.Get<PlainTransactionManagerProvider>());
            //registry.BindToMethod<IPlainTransactionManager>(context => context.Get<PlainPostgresTransactionManager>());

            //registry.Bind(typeof(IPlainStorageAccessor<>), typeof(PostgresPlainStorageRepository<>));
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            try
            {
                status.Message = Modules.InitializingDb;
                DatabaseManagement.InitDatabase(this.settings.ConnectionString, this.settings.SchemaName);

                status.Message = Modules.MigrateDb;
                DbMigrations.DbMigrationsRunner.MigrateToLatest(this.settings.ConnectionString, this.settings.SchemaName, this.settings.DbUpgradeSettings);
            }
            catch (Exception exc)
            {
                LogManager.GetLogger("maigration", typeof(PostgresPlainStorageModule)).Fatal(exc, "Error during db initialization.");
                throw;
            }

            return Task.CompletedTask;
        }


        private ISessionFactory BuildSessionFactory(string schemaName)
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = this.settings.ConnectionString;
                db.Dialect<PostgreSQL91Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.Keywords;
            });

            cfg.AddDeserializedMapping(this.GetMappings(schemaName), "Plain");
            cfg.SetProperty(NHibernate.Cfg.Environment.DefaultSchema, schemaName);
            cfg.SessionFactory().GenerateStatistics();

            var sessionFactory = cfg.BuildSessionFactory();

            MetricsRegistry.Instance.RegisterOnDemandCollectors(
                new NHibernateStatsCollector("plainstore", sessionFactory)
            );

            return sessionFactory;
        }

        private HbmMapping GetMappings(string schemaName)
        {
            var mapper = new ModelMapper();
            var mappingTypes = this.settings.MappingAssemblies
                                            .SelectMany(x => x.GetExportedTypes())
                                            .Where(x => x.GetCustomAttribute<PlainStorageAttribute>() != null &&
                                                        x.IsSubclassOfRawGeneric(typeof(ClassMapping<>)));

            mapper.AddMappings(mappingTypes);
            mapper.BeforeMapProperty += (inspector, member, customizer) =>
            {
                var propertyInfo = (PropertyInfo)member.LocalMember;
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

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }
    }
}
