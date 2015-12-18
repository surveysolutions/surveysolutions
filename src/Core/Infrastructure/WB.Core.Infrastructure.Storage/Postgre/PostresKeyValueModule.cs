using System;
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
        private static ReadSideCacheSettings cacheSettings;

        public PostresKeyValueModule(ReadSideCacheSettings cacheSettings)
        {
            PostresKeyValueModule.cacheSettings = cacheSettings;
        }

        public override void Load()
        {
            this.Kernel.Bind(typeof(IReadSideKeyValueStorage<>))
                .ToMethod(GetReadSideKeyValueStorage)
                .InSingletonScope();

            this.Kernel.Bind(typeof(IPlainKeyValueStorage<>))
                .To(typeof(PostgresPlainKeyValueStorage<>));
        }

        private static object GetReadSideKeyValueStorage(IContext context)
        {
            object keyValueStorage = context.Kernel.GetService(typeof(PostgresReadSideKeyValueStorage<>).MakeGenericType(context.GenericArguments[0]));
            var fileSystemAccessor = context.Kernel.Get<IFileSystemAccessor>();

            Type cachingKeyValueStorageType = typeof(EsentCachedReadSideStorage<>).MakeGenericType(context.GenericArguments[0]);

            object cachingKeyValueStorage = Activator.CreateInstance(cachingKeyValueStorageType,
                keyValueStorage, fileSystemAccessor, cacheSettings);

            return cachingKeyValueStorage;
        }
    }
}