using System.Reflection;
using Ninject;
using Ninject.Activation;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Core.Infrastructure.Storage.Raven
{
    public class RavenReadSideInfrastructureModule : RavenInfrastructureModule
    {
        private readonly Assembly[] assembliesWithIndexes;
        private readonly string basePath;

        public RavenReadSideInfrastructureModule(RavenConnectionSettings settings, string basePath, params Assembly[] assembliesWithIndexes)
            : base(settings)
        {
            this.assembliesWithIndexes = assembliesWithIndexes;
            this.basePath = basePath;
        }

        public override void Load()
        {
            this.BindDocumentStore();

            this.Bind<IReadSideStatusService>().To<RavenReadSideService>().InSingletonScope();
            this.Bind<IReadSideRepositoryIndexAccessor>().To<RavenReadSideRepositoryIndexAccessor>().InSingletonScope()
                .WithConstructorArgument("assembliesWithIndexes", this.assembliesWithIndexes);
            this.Bind<IReadSideAdministrationService>().To<RavenReadSideService>().InSingletonScope();

            // each repository writer should exist in one instance because it might use caching
            this.Kernel.Bind(typeof(RavenReadSideRepositoryWriter<>)).ToSelf().InSingletonScope().WithConstructorArgument("basePath", basePath);

            this.Kernel.Bind(typeof(IReadSideRepositoryReader<>)).ToMethod(this.GetReadSideRepositoryReader);
            this.Kernel.Bind(typeof(IQueryableReadSideRepositoryReader<>)).ToMethod(this.GetReadSideRepositoryReader);
            this.Kernel.Bind(typeof(IReadSideRepositoryWriter<>)).ToMethod(this.GetReadSideRepositoryWriter);
            this.Kernel.Bind(typeof(IQueryableReadSideRepositoryWriter<>)).ToMethod(this.GetReadSideRepositoryWriter);
        }

        protected object GetReadSideRepositoryWriter(IContext context)
        {
            return this.Kernel.Get(typeof(RavenReadSideRepositoryWriter<>).MakeGenericType(context.GenericArguments[0]));
        }

        protected object GetReadSideRepositoryReader(IContext context)
        {
            return this.Kernel.Get(typeof(RavenReadSideRepositoryReader<>).MakeGenericType(context.GenericArguments[0]));
        }
    }
}