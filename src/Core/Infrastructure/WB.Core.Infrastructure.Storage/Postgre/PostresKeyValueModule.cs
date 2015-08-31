using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Postgre
{
    public class PostresKeyValueModule : NinjectModule
    {
        private static int memoryCacheSizePerEntity;

        public PostresKeyValueModule(int memoryCacheSizePerEntity)
        {
            PostresKeyValueModule.memoryCacheSizePerEntity = memoryCacheSizePerEntity;
        }

        public override void Load()
        {
            this.Kernel.Bind(typeof (MemoryCachedKeyValueStorageProvider<>)).ToSelf();

            this.Kernel.Bind(typeof (IReadSideKeyValueStorage<>))
                .ToMethod(GetReadSideKeyValueStorage)
                .InSingletonScope();

            this.Kernel.Bind(typeof (IPlainKeyValueStorage<>))
                .To(typeof(PostgresPlainKeyValueStorage<>));
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
                var esentKeyValueStorage = new PostgresReadSideKeyValueStorage<TEntity>(
                    context.Kernel.Get<ISessionProvider>(PostgresReadSideModule.SessionProviderName),
                                                        context.Kernel.Get<PostgreConnectionSettings>());

                return new MemoryCachedKeyValueStorage<TEntity>(
                    esentKeyValueStorage,
                    new ReadSideStoreMemoryCacheSettings(memoryCacheSizePerEntity, memoryCacheSizePerEntity / 2));
            }
        }
    }
}