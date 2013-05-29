using Ninject.Modules;

using WB.Core.Infrastructure.Implementation;

namespace WB.Core.Infrastructure
{
    public class CoreInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IReadLayerStatusService>().To<ReadLayerService>().InSingletonScope();
            this.Bind<IReadLayerAdministrationService>().To<ReadLayerService>().InSingletonScope();
        }
    }
}