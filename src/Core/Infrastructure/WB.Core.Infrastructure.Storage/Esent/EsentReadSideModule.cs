using Ninject.Modules;
using ServiceStack.Redis;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Esent.Implementation;
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
            this.Kernel.Bind(typeof(IReadSideKeyValueStorage<>)).To(typeof(EsentKeyValueStorage<>)).InSingletonScope();
        }
    }
}