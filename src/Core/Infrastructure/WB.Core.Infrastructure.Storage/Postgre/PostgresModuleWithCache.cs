using System;
using System.Reflection;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Esent.Implementation;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Postgre
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
                .GetMethod(nameof(CreateEsentCachedReadSideStorage), BindingFlags.NonPublic | BindingFlags.Instance)
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

        public override void Load()
        {
            if (!this.Kernel.HasBinding<ReadSideCacheSettings>())
            {
                this.Bind<ReadSideCacheSettings>().ToConstant(this.cacheSettings);
            }
        }
    }
}