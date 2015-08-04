using Ninject.Modules;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.Tester
{
    public class TesterBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IDesignerApiService>().To<DesignerApiService>().InSingletonScope();
            this.Bind<IFriendlyMessageService>().To<FriendlyMessageService>().InSingletonScope();

            
            this.Bind<ISubstitutionService>().To<SubstitutionService>();
            this.Bind<IGpsLocationService>().To<GpsLocationService>().InSingletonScope();
            this.Bind<IViewModelNavigationService>().To<ViewModelNavigationService>();
        }
    }
}
