using System.Reflection;
using Ninject;
using Ninject.Activation;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Services;
using WB.Core.Infrastructure.Storage.Esent.Implementation;
using WB.Core.Infrastructure.Storage.Memory.Implementation;
using WB.Core.Infrastructure.Storage.Raven.Implementation;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.Infrastructure.Storage.Raven
{
    public class RavenReadSideInfrastructureModule : RavenInfrastructureModule
    {
        private readonly Assembly[] assembliesWithIndexes;
        private readonly RavenReadSideRepositoryWriterSettings ravenReadSideRepositoryWriterSettings;
        private static int memoryCacheSizePerEntity;

        public RavenReadSideInfrastructureModule(RavenConnectionSettings settings, RavenReadSideRepositoryWriterSettings ravenReadSideRepositoryWriterSettings,
            int memoryCacheSizePerEntity, params Assembly[] assembliesWithIndexes)
            : base(settings)
        {
            this.assembliesWithIndexes = assembliesWithIndexes;
            this.ravenReadSideRepositoryWriterSettings = ravenReadSideRepositoryWriterSettings;
            RavenReadSideInfrastructureModule.memoryCacheSizePerEntity = memoryCacheSizePerEntity;
        }

        public override void Load()
        {
            this.BindDocumentStore();

            this.Bind<IReadSideRepositoryIndexAccessor>().To<RavenReadSideRepositoryIndexAccessor>().InSingletonScope()
                .WithConstructorArgument("assembliesWithIndexes", this.assembliesWithIndexes);

            
            this.Bind<RavenReadSideRepositoryWriterSettings>().ToConstant(ravenReadSideRepositoryWriterSettings);
            this.Bind<IReadSideCleaner>().To<ReadSideCleaner>().InSingletonScope()
                .WithConstructorArgument("assembliesWithIndexes", this.assembliesWithIndexes);

          
            this.Kernel.Bind(typeof(IReadSideRepositoryReader<>)).ToMethod(this.GetReadSideRepositoryReader);
            this.Kernel.Bind(typeof(IQueryableReadSideRepositoryReader<>)).ToMethod(this.GetReadSideRepositoryReader);

            // each repository writer should exist in one instance because it might use caching
            this.Kernel.Bind(typeof(RavenReadSideRepositoryWriter<>)).ToSelf().InSingletonScope();
            this.Kernel.Bind(typeof(MemoryCachedReadSideRepositoryWriterProvider<>)).ToSelf();
            this.Kernel.Bind(typeof(IReadSideRepositoryWriter<>)).ToMethod(GetReadSideRepositoryWriter).InSingletonScope(); 
        }

        protected object GetReadSideRepositoryReader(IContext context)
        {
            return this.Kernel.Get(typeof(RavenReadSideRepositoryReader<>).MakeGenericType(context.GenericArguments[0]));
        }

        protected object GetReadSideRepositoryWriter(IContext context)
        {
            var genericProvider = this.Kernel.Get(
                  typeof(MemoryCachedReadSideRepositoryWriterProvider<>).MakeGenericType(context.GenericArguments[0])) as
                  IProvider;

            if (genericProvider == null)
                return null;
            return genericProvider.Create(context);
        }

        private class MemoryCachedReadSideRepositoryWriterProvider<TEntity> : Provider<IReadSideRepositoryWriter<TEntity>>
           where TEntity : class, IReadSideRepositoryEntity
        {
            protected override IReadSideRepositoryWriter<TEntity> CreateInstance(IContext context)
            {
                return new MemoryCachedReadSideRepositoryWriter<TEntity>(
                    context.Kernel.Get<RavenReadSideRepositoryWriter<TEntity>>(),
                    new ReadSideStoreMemoryCacheSettings(memoryCacheSizePerEntity, memoryCacheSizePerEntity / 2));
            }
        }
    }
}