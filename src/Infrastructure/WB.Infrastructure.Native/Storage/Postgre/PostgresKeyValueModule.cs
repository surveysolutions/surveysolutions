using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class PostgresKeyValueModule : PostgresModuleWithCache
    {
        public PostgresKeyValueModule(ReadSideCacheSettings cacheSettings)
            : base(cacheSettings) {}

        protected override IReadSideStorage<TEntity> GetPostgresReadSideStorage<TEntity>(IModuleContext context)
            => (IReadSideStorage<TEntity>) context.GetServiceWithGenericType(typeof(PostgresReadSideKeyValueStorage<>), typeof(TEntity));

        protected override IReadSideStorage<TEntity, TKey> GetPostgresReadSideStorage<TEntity, TKey>(IModuleContext context)
        {
            return (IReadSideStorage<TEntity, TKey>)context.GetServiceWithGenericType(typeof(PostgresReadSideKeyValueStorage<>), typeof(TEntity), typeof(TKey));
        }

        public override void Load(IIocRegistry registry)
        {
            base.Load(registry);

            registry.Bind(typeof(IReadSideKeyValueStorage<>), typeof(PostgresReadSideKeyValueStorage<>));
            registry.Bind(typeof(IReadSideKeyValueStorage<,>), typeof(PostgresReadSideKeyValueStorage<>));

            registry.Bind(typeof(IPlainKeyValueStorage<>), typeof(PostgresPlainKeyValueStorage<>));

            registry.BindAsSingleton(typeof(IEntitySerializer<>), typeof(EntitySerializer<>));
        }
    }
}
