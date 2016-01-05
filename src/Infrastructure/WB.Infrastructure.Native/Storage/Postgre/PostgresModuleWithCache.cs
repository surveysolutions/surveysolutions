using System;
using System.Reflection;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Infrastructure.Native.Storage.Esent.Implementation;
using WB.Infrastructure.Native.Storage.Memory.Implementation;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public abstract class PostgresModuleWithCache : NinjectModule
    {
        private readonly ReadSideCacheSettings cacheSettings;

        protected PostgresModuleWithCache(ReadSideCacheSettings cacheSettings)
        {
            this.cacheSettings = cacheSettings;
        }

        protected abstract IReadSideStorage<TEntity> GetPostgresReadSideStorage<TEntity>(IContext context)
            where TEntity : class, IReadSideRepositoryEntity;

        protected object GetReadSideStorageWrappedWithCache(IContext context)
        {
            Type storageEntityType = context.GenericArguments[0];

            MethodInfo createCachingStorageMethod = typeof(PostgresModuleWithCache)
                .GetMethod(
                    this.cacheSettings.EnableEsentCache
                        ? nameof(this.CreateEsentCachedReadSideStorage)
                        : nameof(this.CreateMemoryCachedReadSideStorage),
                    BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(storageEntityType);

            return createCachingStorageMethod.Invoke(this, new object[] { context });
        }

        private EsentCachedReadSideStorage<TEntity> CreateEsentCachedReadSideStorage<TEntity>(IContext context)
            where TEntity : class, IReadSideRepositoryEntity
        {
            IReadSideStorage<TEntity> postgresStorage = this.GetPostgresReadSideStorage<TEntity>(context);
            var fileSystemAccessor = context.Kernel.Get<IFileSystemAccessor>();

            return new EsentCachedReadSideStorage<TEntity>(postgresStorage, fileSystemAccessor, this.cacheSettings);
        }

        private MemoryCachedReadSideStorage<TEntity> CreateMemoryCachedReadSideStorage<TEntity>(IContext context)
            where TEntity : class, IReadSideRepositoryEntity
        {
            IReadSideStorage<TEntity> postgresStorage = this.GetPostgresReadSideStorage<TEntity>(context);

            return new MemoryCachedReadSideStorage<TEntity>(postgresStorage, this.cacheSettings);
        }

        public override void Load()
        {
            if (!this.Kernel.HasBinding<ReadSideCacheSettings>())
            {
                this.Bind<ReadSideCacheSettings>().ToConstant(this.cacheSettings);
            }
        }
    }
}