using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Esent.Implementation;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using WB.Core.SharedKernels.SurveySolutions;

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

            this.Kernel.Bind(typeof (EsentKeyValueStorage<>)).ToSelf().InSingletonScope();

            this.Kernel.Bind(typeof (MemoryCachedKeyValueStorageProvider<>)).ToSelf();

            this.Kernel.Bind(typeof (IReadSideKeyValueStorage<>))
                .ToMethod(GetReadSideKeyValueStorage)
                .InSingletonScope();
        }

        protected object GetReadSideKeyValueStorage(IContext context)
        {
            var genericProvider = this.Kernel.Get(
                typeof (MemoryCachedKeyValueStorageProvider<>).MakeGenericType(context.GenericArguments[0])) as
                IProvider;

            if(genericProvider==null)
                return null;
            return genericProvider.Create(context);
        }

        private class MemoryCachedKeyValueStorageProvider<TEntity> : Provider<IReadSideKeyValueStorage<TEntity>>
            where TEntity : class, IReadSideRepositoryEntity
        {
            protected override IReadSideKeyValueStorage<TEntity> CreateInstance(IContext context)
            {
                return new MemoryCachedKeyValueStorage<TEntity>(context.Kernel.Get<EsentKeyValueStorage<TEntity>>());
            }
        }
    }
}