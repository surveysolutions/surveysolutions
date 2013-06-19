using Ninject.Modules;

using WB.Core.Infrastructure.Raven.Implementation;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.Infrastructure.Raven
{
    public class RavenInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IReadSideStatusService>().To<RavenReadSideService>().InSingletonScope();
            this.Bind<IReadSideAdministrationService>().To<RavenReadSideService>().InSingletonScope();
        }
    }
}