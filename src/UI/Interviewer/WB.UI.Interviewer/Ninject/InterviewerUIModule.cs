using Ninject.Modules;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Interviewer.Implementations.Services;

namespace WB.UI.Interviewer.Ninject
{
    public class InterviewerUIModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IViewModelNavigationService>().To<ViewModelNavigationService>();
            this.Bind<ITabletDiagnosticService>().To<TabletDiagnosticService>();

#if EXCLUDEEXTENSIONS
            this.Bind<IAreaEditService>().To<WB.UI.Shared.Enumerator.CustomServices.AreaEditor.DummyAreaEditService>();
#else
            this.Bind<IAreaEditService>().To<WB.UI.Shared.Extensions.CustomServices.AreaEditor.AreaEditService>();
#endif
        }
    }
}