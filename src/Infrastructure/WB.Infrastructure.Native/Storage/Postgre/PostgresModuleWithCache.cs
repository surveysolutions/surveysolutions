using System;
using System.Reflection;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Infrastructure.Native.Storage.Memory.Implementation;

namespace WB.Infrastructure.Native.Storage.Postgre
{
    public abstract class PostgresModuleWithCache : IModule
    {
        private readonly ReadSideCacheSettings cacheSettings;

        protected PostgresModuleWithCache(ReadSideCacheSettings cacheSettings)
        {
            this.cacheSettings = cacheSettings;
        }

        protected abstract IReadSideStorage<TEntity> GetPostgresReadSideStorage<TEntity>(IModuleContext context)
            where TEntity : class, IReadSideRepositoryEntity;

        protected abstract IReadSideStorage<TEntity, TKey> GetPostgresReadSideStorage<TEntity, TKey>(IModuleContext context)
            where TEntity : class, IReadSideRepositoryEntity;

        protected object GetReadSideStorageWrappedWithCache(IModuleContext context)
        {
            Type storageEntityType = context.GetGenericArgument();

            MethodInfo createCachingStorageMethod = typeof(PostgresModuleWithCache)
                .GetMethod(nameof(this.CreateMemoryCachedReadSideStorage),
                    BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(storageEntityType);

            return createCachingStorageMethod.Invoke(this, new object[] { context });
        }

        protected object GetGenericReadSideStorageWrappedWithCache(IModuleContext context)
        {
            var storageEntityType = context.GetGenericArguments();

            MethodInfo createCachingStorageMethod = typeof(PostgresModuleWithCache)
                .GetMethod(nameof(this.CreateGenericMemoryCachedReadSideStorage),
                    BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(storageEntityType);

            return createCachingStorageMethod.Invoke(this, new object[] { context });
        }

        private MemoryCachedReadSideStorage<TEntity> CreateMemoryCachedReadSideStorage<TEntity>(IModuleContext context)
            where TEntity : class, IReadSideRepositoryEntity
        {
            IReadSideStorage<TEntity> postgresStorage = this.GetPostgresReadSideStorage<TEntity>(context);

            return new MemoryCachedReadSideStorage<TEntity>(postgresStorage, this.cacheSettings);
        }

        private MemoryCachedReadSideStorage<TEntity, TKey> CreateGenericMemoryCachedReadSideStorage<TEntity, TKey>(IModuleContext context)
            where TEntity : class, IReadSideRepositoryEntity
        {
            IReadSideStorage<TEntity, TKey> postgresStorage = this.GetPostgresReadSideStorage<TEntity, TKey>(context);

            return new MemoryCachedReadSideStorage<TEntity, TKey>(postgresStorage, this.cacheSettings);
        }

        public virtual void Load(IIocRegistry registry)
        {
            if (!registry.HasBinding<ReadSideCacheSettings>())
            {
                registry.BindToConstant(() => this.cacheSettings);
            }
        }

        public virtual Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
