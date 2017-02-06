using System;
using System.Linq;
using System.Reflection;
using Humanizer;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using Ninject;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class PostgresPlainStorageModule : Ninject.Modules.NinjectModule
    {
        internal const string SessionFactoryName = "PlainSessionFactory";
        private readonly PostgresPlainStorageSettings settings;

        public PostgresPlainStorageModule(PostgresPlainStorageSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            this.settings = settings;
        }

        public override void Load()
        {
            this.Bind<PostgresPlainStorageSettings>().ToConstant(this.settings);
            
            try
            {
                DatabaseManagement.InitDatabase(this.settings.ConnectionString, this.settings.SchemaName);
            }
            catch (Exception exc)
            {
                this.Kernel.Get<ILogger>().Fatal("Error during db initialization.", exc);
                throw;
            }

            this.Bind<ISessionFactory>().ToMethod(context => this.BuildSessionFactory(settings.SchemaName))
                                        .InSingletonScope()
                                        .Named(SessionFactoryName);

            this.Kernel.Bind<PlainPostgresTransactionManager>().ToSelf().InIsolatedThreadScopeOrRequestScopeOrThreadScope();
            this.Kernel.Bind<NoTransactionPlainPostgresTransactionManager>().ToSelf().InIsolatedThreadScopeOrRequestScopeOrThreadScope();

            this.Kernel.Bind<Func<PlainPostgresTransactionManager>>().ToMethod(context => () => context.Kernel.Get<PlainPostgresTransactionManager>());
            this.Kernel.Bind<Func<NoTransactionPlainPostgresTransactionManager>>().ToMethod(context => () => context.Kernel.Get<NoTransactionPlainPostgresTransactionManager>());

            this.Kernel.Bind<RebuildReadSidePlainPostgresTransactionManagerWithSessions>().ToSelf();

            this.Kernel
                .Bind<PlainTransactionManagerProvider>()
                .ToConstructor(constructor => new PlainTransactionManagerProvider(
                    constructor.Inject<Func<PlainPostgresTransactionManager>>(),
                    constructor.Inject<Func<NoTransactionPlainPostgresTransactionManager>>(),
                    constructor.Inject<RebuildReadSidePlainPostgresTransactionManagerWithSessions>()))
                .InSingletonScope();

            this.Kernel.Bind<IPlainSessionProvider>().ToMethod(context => context.Kernel.Get<PlainTransactionManagerProvider>());
            this.Kernel.Bind<IPlainTransactionManager>().ToMethod(context => context.Kernel.Get<PlainPostgresTransactionManager>());

            this.Kernel.Bind<IPlainTransactionManagerProvider>().ToMethod(context => context.Kernel.Get<PlainTransactionManagerProvider>());

            this.Bind(typeof(IPlainStorageAccessor<>)).To(typeof(PostgresPlainStorageRepository<>));

            DbMigrations.DbMigrationsRunner.MigrateToLatest(this.settings.ConnectionString, this.settings.SchemaName, this.settings.DbUpgradeSettings);
        }

        private ISessionFactory BuildSessionFactory(string schemaName)
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = this.settings.ConnectionString;
                db.Dialect<PostgreSQL91Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
            });

            cfg.AddDeserializedMapping(this.GetMappings(schemaName), "Plain");
            cfg.SetProperty(NHibernate.Cfg.Environment.DefaultSchema, schemaName);
            return cfg.BuildSessionFactory();
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