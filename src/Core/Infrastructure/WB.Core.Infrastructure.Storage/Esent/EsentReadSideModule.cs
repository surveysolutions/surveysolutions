using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using ServiceStack.Redis;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Esent.Implementation;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.Storage.RedisStore.Implementation;

namespace WB.Core.Infrastructure.Storage.Esent
{
    public class EsentReadSideModule : NinjectModule
    {
        private readonly string dataFolder;

        public EsentReadSideModule(string dataFolder)
        {
            this.dataFolder = dataFolder;
        }

        public override void Load()
        {
            this.Kernel.Bind<EsentSettings>().ToConstant(new EsentSettings(dataFolder));

            this.Kernel.Bind(typeof(EsentKeyValueStorage<>)).ToSelf().InSingletonScope();

            this.Kernel.Bind(typeof(MemoryCachedKeyValueStorage<>)).ToSelf().InSingletonScope();

            this.Kernel.Bind(typeof(IReadSideKeyValueStorage<>)).To(typeof(MemoryCachedKeyValueStorage<>)).InSingletonScope();
        }
    }
}