using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Humanizer;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Tool.hbm2ddl;
using Ninject;
using Ninject.Activation;
using Ninject.Planning.Targets;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Infrastructure.Native.Storage.Postgre.DbMigrations;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class PostgresReadSideModule : PostgresModuleWithCache
    {
        public const string ReadSideSessionFactoryName = "ReadSideSessionFactory";
        internal const string SessionProviderName = "ReadSideProvider";
        private readonly string connectionString;
        private readonly string schemaName;
        private readonly DbUpgradeSettings dbUpgradeSettings;
        private readonly IEnumerable<Assembly> mappingAssemblies;

        public PostgresReadSideModule(string connectionString, 
            string schemaName,
            DbUpgradeSettings dbUpgradeSettings,
            ReadSideCacheSettings cacheSettings,
            IEnumerable<Assembly> mappingAssemblies)
            : base(cacheSettings)
        {
            this.connectionString = connectionString;
            this.schemaName = schemaName;
            this.dbUpgradeSettings = dbUpgradeSettings;
            this.mappingAssemblies = mappingAssemblies;
        }

        protected override IReadSideStorage<TEntity> GetPostgresReadSideStorage<TEntity>(IContext context)
            => (IReadSideStorage<TEntity>) context.Kernel.GetService(typeof(PostgreReadSideStorage<>).MakeGenericType(typeof(TEntity)));

        public override void Load()
        {
            base.Load();

            try
            {
                DatabaseManagement.InitDatabase(this.connectionString, this.schemaName);
            }
            catch (Exception exc)
            {
                this.Kernel.Get<ILogger>().Fatal("Error during db initialization.", exc);
                throw;
            }

            this.Kernel.Bind<PostgreConnectionSettings>().ToConstant(new PostgreConnectionSettings
            {
                ConnectionString = this.connectionString,
                SchemaName = "readside",
            });

            this.Kernel.Bind<IPostgresReadSideBootstraper>().To<PostgresReadSideBootstraper>()
                                                            .WithConstructorArgument("dbUpgradeSettings", this.dbUpgradeSettings);

            this.Kernel.Bind(typeof(PostgreReadSideStorage<>), typeof(IQueryableReadSideRepositoryReader<>),
                typeof(IReadSideRepositoryReader<>), typeof(INativeReadSideStorage<>))
                .ToSelf()
                .InSingletonScope()
                .WithConstructorArgument("entityIdentifierColumnName", GetEntityIdentifierColumnName);

            this.Kernel.Bind(typeof(IReadSideRepositoryWriter<>)).ToMethod(this.GetReadSideStorageWrappedWithCache).InSingletonScope(); 
            
            this.Kernel.Bind<ISessionFactory>()
                       .ToMethod(kernel => this.BuildSessionFactory())
                       .InSingletonScope()
                       .Named(ReadSideSessionFactoryName);

            this.Kernel.Bind<CqrsPostgresTransactionManager>().ToSelf().InIsolatedThreadScopeOrRequestScopeOrThreadScope();
            this.Kernel.Bind<NoTransactionCqrsPostgresTransactionManager>().ToSelf().InIsolatedThreadScopeOrRequestScopeOrThreadScope();

            this.Kernel.Bind<Func<CqrsPostgresTransactionManager>>().ToMethod(context => () => context.Kernel.Get<CqrsPostgresTransactionManager>());
            this.Kernel.Bind<Func<NoTransactionCqrsPostgresTransactionManager>>().ToMethod(context => () => context.Kernel.Get<NoTransactionCqrsPostgresTransactionManager>());
            
            this.Kernel.Bind<RebuildReadSideCqrsPostgresTransactionManagerWithSessions>().ToSelf();
            this.Kernel.Bind<RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions>().ToSelf();

            this.Kernel
                .Bind<TransactionManagerProvider>()
                .ToConstructor(constructor => new TransactionManagerProvider(
                    constructor.Inject<Func<CqrsPostgresTransactionManager>>(),
                    constructor.Inject<Func<NoTransactionCqrsPostgresTransactionManager>>(),
                    constructor.Inject<RebuildReadSideCqrsPostgresTransactionManagerWithSessions>(),
                    constructor.Inject<RebuildReadSideCqrsPostgresTransactionManagerWithoutSessions>(),
                    constructor.Inject<ReadSideCacheSettings>()))
                .InSingletonScope();

            this.Kernel.Bind<ISessionProvider>().ToMethod(context => context.Kernel.Get<TransactionManagerProvider>()).Named(SessionProviderName);
            this.Kernel.Bind<ITransactionManager>().ToMethod(context => context.Kernel.Get<CqrsPostgresTransactionManager>());

            this.Kernel.Bind<ITransactionManagerProvider>().ToMethod(context => context.Kernel.Get<TransactionManagerProvider>());
            this.Kernel.Bind<ITransactionManagerProviderManager>().ToMethod(context => context.Kernel.Get<TransactionManagerProvider>());
            
            DbMigrationsRunner.MigrateToLatest(this.connectionString, this.schemaName, this.dbUpgradeSettings);
        }

        private object GetEntityIdentifierColumnName(IContext context, ITarget target)
        {
            var entityType = context.GenericArguments[0];

            var sessionFactory = context.Kernel.Get<ISessionFactory>(ReadSideSessionFactoryName);

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
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
            });
            cfg.SetProperty(NHibernate.Cfg.Environment.WrapResultSets, "true");
            cfg.AddDeserializedMapping(this.GetMappings(), "Main");

            return cfg.BuildSessionFactory();
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