using Ninject.Modules;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.UI.Interviewer.Implementations.Services;

namespace WB.UI.Interviewer.Ninject
{
    public class InterviewerUIModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IViewModelNavigationService>().To<ViewModelNavigationService>();
        }
    }
}