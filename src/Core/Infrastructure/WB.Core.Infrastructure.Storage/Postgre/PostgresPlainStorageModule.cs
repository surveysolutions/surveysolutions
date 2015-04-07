using System;
using System.Linq;
using System.Reflection;
using Humanizer;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Tool.hbm2ddl;
using Ninject;
using Ninject.Web.Common;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;

namespace WB.Core.Infrastructure.Storage.Postgre
{
    public class PostgresPlainStorageModule : Ninject.Modules.NinjectModule
    {
        internal const string SessionFactoryName = "PlainSessionFactory";
        internal const string SessionProviderName = "PlainSessinProvider";
        private readonly PostgresPlainStorageSettings settings;

        public PostgresPlainStorageModule(PostgresPlainStorageSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            this.settings = settings;
        }

        public override void Load()
        {
            this.Bind<ISessionFactory>().ToMethod(context => this.BuildSessionFactory())
                                        .InSingletonScope()
                                        .Named(SessionFactoryName);

            this.Bind<PlainPostgresTransactionManager>()
                .ToSelf()
                .InRequestScope();

            this.Bind<IPlainSessionProvider>().ToMethod(context => context.Kernel.Get<PlainPostgresTransactionManager>());
            this.Bind<IPlainTransactionManager>().ToMethod(context => context.Kernel.Get<PlainPostgresTransactionManager>());
            this.Bind(typeof(IPlainStorageAccessor<>)).To(typeof(PostgresPlainStorageRepository<>));
        }

        private ISessionFactory BuildSessionFactory()
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = this.settings.ConnectionString;
                db.Dialect<NHibernate.Dialect.PostgreSQL82Dialect>();
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
            });
            cfg.Proxy(proxy => proxy.ProxyFactoryFactory<NHibernate.ByteCode.Castle.ProxyFactoryFactory>());

            cfg.AddDeserializedMapping(GetMappings(), "Plain");
            var update = new SchemaUpdate(cfg);
            update.Execute(true, true);

            return cfg.BuildSessionFactory();
        }

        private HbmMapping GetMappings()
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
            };

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }
    }

    
}