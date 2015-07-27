using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Esent.Implementation;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Postgre
{
    public class PostresKeyValueModule : NinjectModule
    {
        private static string readSideConnectionString;
        private static int memoryCacheSizePerEntity;
        private static string plainStorageConnectionString;

        public PostresKeyValueModule(string readSideConnectionString, string plainStorageConnectionString, int memoryCacheSizePerEntity)
        {
            PostresKeyValueModule.readSideConnectionString = readSideConnectionString;
            PostresKeyValueModule.memoryCacheSizePerEntity = memoryCacheSizePerEntity;
            PostresKeyValueModule.plainStorageConnectionString = plainStorageConnectionString;
        }

        public override void Load()
        {
            this.Kernel.Bind(typeof (MemoryCachedKeyValueStorageProvider<>)).ToSelf();

            this.Kernel.Bind(typeof (IReadSideKeyValueStorage<>))
                .ToMethod(GetReadSideKeyValueStorage)
                .InSingletonScope();

            this.Kernel.Bind(typeof (IPlainKeyValueStorage<>))
                .To(typeof(PostgreKeyValueStorage<>))
                .InSingletonScope()
                .WithConstructorArgument(new PostgreConnectionSettings{ConnectionString = plainStorageConnectionString});
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
                var esentKeyValueStorage = new PostgreKeyValueStorage<TEntity>(new PostgreConnectionSettings { ConnectionString = readSideConnectionString });

                return new MemoryCachedKeyValueStorage<TEntity>(
                    esentKeyValueStorage,
                    new ReadSideStoreMemoryCacheSettings(memoryCacheSizePerEntity, memoryCacheSizePerEntity / 2));
            }
        }
    }
}