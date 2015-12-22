using System.Linq;
using Ninject.Activation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Postgre
{
    public class PostgresKeyValueModule : PostgresModuleWithCache
    {
        public PostgresKeyValueModule(ReadSideCacheSettings cacheSettings)
            : base(cacheSettings) {}

        protected override object GetPostgresReadSideStorage(IContext context)
            => context.Kernel.GetService(typeof(PostgresReadSideKeyValueStorage<>).MakeGenericType(context.GenericArguments[0]));

        public override void Load()
        {
            base.Load();

            this.Kernel.Bind(typeof(IReadSideKeyValueStorage<>))
                .ToMethod(this.GetReadSideStorageWrappedWithCache)
                .InSingletonScope();

            this.Kernel.Bind(typeof(IPlainKeyValueStorage<>))
                .To(typeof(PostgresPlainKeyValueStorage<>));
        }
    }
}