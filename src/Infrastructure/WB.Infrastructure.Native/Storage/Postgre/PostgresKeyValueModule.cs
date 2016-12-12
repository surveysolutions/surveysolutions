using Ninject.Activation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public class PostgresKeyValueModule : PostgresModuleWithCache
    {
        public PostgresKeyValueModule(ReadSideCacheSettings cacheSettings)
            : base(cacheSettings) {}

        protected override IReadSideStorage<TEntity> GetPostgresReadSideStorage<TEntity>(IContext context)
            => (IReadSideStorage<TEntity>) context.Kernel.GetService(typeof(PostgresReadSideKeyValueStorage<>).MakeGenericType(typeof(TEntity)));

        public override void Load()
        {
            base.Load();

            this.Kernel.Bind(typeof(IReadSideKeyValueStorage<>))
                .ToMethod(this.GetReadSideStorageWrappedWithCache)
                .InSingletonScope();

            this.Kernel.Bind(typeof(IPlainKeyValueStorage<>))
                .To(typeof(PostgresPlainKeyValueStorage<>))
                .InSingletonScope();

            this.Kernel.Bind(typeof(IEntitySerializer<>))
                .To(typeof(EntitySerializer<>))
                .InSingletonScope();
        }
    }
}