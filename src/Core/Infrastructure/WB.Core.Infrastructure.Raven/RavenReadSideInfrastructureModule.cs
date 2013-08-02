using WB.Core.Infrastructure.Raven.Implementation;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.Infrastructure.Raven
{
    public class RavenReadSideInfrastructureModule : RavenInfrastructureModule
    {
        public RavenReadSideInfrastructureModule(RavenConnectionSettings settings)
            : base(settings) {}

        public override void Load()
        {
            this.BindDocumentStore();

            this.Bind<IReadSideStatusService>().To<RavenReadSideService>().InSingletonScope();
            this.Bind<IReadSideRepositoryIndexAccessor>().To<RavenReadSideRepositoryIndexAccessor>().InSingletonScope();
            this.Bind<IReadSideAdministrationService>().To<RavenReadSideService>().InSingletonScope();

            this.Bind<IRavenReadSideRepositoryWriterRegistry>().To<RavenReadSideRepositoryWriterRegistry>().InSingletonScope();

            // each repository writer should exist in one instance because it might use caching
            this.Kernel.Bind(typeof(RavenReadSideRepositoryWriter<>)).ToSelf().InSingletonScope();
        }
    }
}