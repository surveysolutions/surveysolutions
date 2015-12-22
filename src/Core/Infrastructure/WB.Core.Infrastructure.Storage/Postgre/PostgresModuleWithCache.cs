using System;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Storage.Esent.Implementation;

namespace WB.Core.Infrastructure.Storage.Postgre
{
    public abstract class PostgresModuleWithCache : NinjectModule
    {
        private readonly ReadSideCacheSettings cacheSettings;

        protected PostgresModuleWithCache(ReadSideCacheSettings cacheSettings)
        {
            this.cacheSettings = cacheSettings;
        }

        protected abstract object GetPostgresReadSideStorage(IContext context);

        protected object GetReadSideStorageWrappedWithCache(IContext context)
        {
            object postgresStorage = this.GetPostgresReadSideStorage(context);
            var fileSystemAccessor = context.Kernel.Get<IFileSystemAccessor>();

            Type cachingStorageType = typeof(EsentCachedReadSideStorage<>).MakeGenericType(context.GenericArguments[0]);

            object cachingStorage = Activator.CreateInstance(cachingStorageType,
                postgresStorage, fileSystemAccessor, this.cacheSettings);

            return cachingStorage;
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