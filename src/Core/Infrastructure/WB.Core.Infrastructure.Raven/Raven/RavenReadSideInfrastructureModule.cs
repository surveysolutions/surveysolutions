using System.Reflection;
using Ninject;
using Ninject.Activation;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.Raven.Implementation;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Raven
{
    public class RavenReadSideInfrastructureModule : RavenInfrastructureModule
    {
        private readonly Assembly[] assembliesWithIndexes;

        public RavenReadSideInfrastructureModule(RavenConnectionSettings settings, params Assembly[] assembliesWithIndexes)
            : base(settings)
        {
            this.assembliesWithIndexes = assembliesWithIndexes;
        }

        public override void Load()
        {
            this.BindDocumentStore();

            this.Bind<IReadSideStatusService>().To<RavenReadSideService>().InSingletonScope();
            this.Bind<IReadSideRepositoryIndexAccessor>().To<RavenReadSideRepositoryIndexAccessor>().InSingletonScope()
                .WithConstructorArgument("assembliesWithIndexes", this.assembliesWithIndexes);
            this.Bind<IReadSideAdministrationService>().To<RavenReadSideService>().InSingletonScope();

            this.Bind<IRavenReadSideRepositoryWriterRegistry>().To<RavenReadSideRepositoryWriterRegistry>().InSingletonScope();

            this.Bind<IReadSideRepositoryCleanerRegistry>().To<ReadSideRepositoryCleanerRegistry>().InSingletonScope();

            // each repository writer should exist in one instance because it might use caching
            this.Kernel.Bind(typeof(RavenReadSideRepositoryWriter<>)).ToSelf().InSingletonScope();

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