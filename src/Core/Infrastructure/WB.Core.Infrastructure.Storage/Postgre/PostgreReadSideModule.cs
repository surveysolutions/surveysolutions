using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using Ninject;
using Ninject.Activation;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.Infrastructure.Storage.Postgre.Implementation.Mapping;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Postgre
{
    public class PostgreModule : Ninject.Modules.NinjectModule
    {
        private readonly string connectionString;

        public PostgreModule(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public override void Load()
        {
            this.Kernel.Bind<PostgreConnectionSettings>().ToConstant(new PostgreConnectionSettings{ConnectionString = connectionString });

            this.Kernel.Bind(typeof(PostgreKeyValueStorage<>)).ToSelf().InSingletonScope();

            this.Kernel.Bind(typeof(MemoryCachedKeyValueStorageProvider<>)).ToSelf();

            this.Kernel.Bind<ISessionFactory>()
                       .ToMethod(kernel => this.BuildSessionFactory())
                       .InSingletonScope();

            this.Kernel.Bind(typeof(IReadSideKeyValueStorage<>))
                .ToMethod(GetReadSideKeyValueStorage)
                .InSingletonScope();
        }

        private ISessionFactory BuildSessionFactory()
        {
            var cfg = new Configuration();
            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = connectionString;
                db.Dialect<NHibernate.Dialect.PostgreSQLDialect>();
            });

            cfg.AddDeserializedMapping(GetMappings(), "Main");

            return cfg.BuildSessionFactory();
        }

        private static HbmMapping GetMappings()
        {
            var mapper = new ModelMapper();

            mapper.AddMapping<StoredEntityMap>();
            var mapping = mapper.CompileMappingFor(new[] { typeof(StoredEntity) });
            return mapping;
        }

        private object GetReadSideKeyValueStorage(IContext context)
        {
            var genericProvider = this.Kernel.Get(
                typeof(MemoryCachedKeyValueStorageProvider<>).MakeGenericType(context.GenericArguments[0])) as
                IProvider;

            if (genericProvider == null)
                return null;
            return genericProvider.Create(context);
        }

        private class MemoryCachedKeyValueStorageProvider<TEntity> : Provider<IReadSideKeyValueStorage<TEntity>>
            where TEntity : class, IReadSideRepositoryEntity
        {
            protected override IReadSideKeyValueStorage<TEntity> CreateInstance(IContext context)
            {
                return new MemoryCachedKeyValueStorage<TEntity>(context.Kernel.Get<PostgreKeyValueStorage<TEntity>>());
            }
        }
    }
}