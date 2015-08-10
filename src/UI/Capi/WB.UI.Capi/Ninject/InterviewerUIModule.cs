using Ninject.Modules;
using WB.Core.BoundedContexts.Tester.Services;
using WB.UI.Capi.Implementations.Services;

namespace WB.UI.Capi.Ninject
{
    public class InterviewerUIModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IViewModelNavigationService>().To<ViewModelNavigationService>();
        }
    }
}