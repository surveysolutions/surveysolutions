using Ninject.Modules;

using WB.Core.Infrastructure.Raven.Implementation;

namespace WB.Core.Infrastructure.Raven
{
    public class RavenInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IReadLayerStatusService>().To<RavenReadLayerService>().InSingletonScope();
            this.Bind<IReadLayerAdministrationService>().To<RavenReadLayerService>().InSingletonScope();
        }
    }
}