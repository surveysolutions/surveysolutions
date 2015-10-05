using Ninject.Modules;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.UI.Interviewer.Syncronization;

namespace WB.UI.Interviewer
{
    public class InterviewerBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ICapiCleanUpService>().To<CapiCleanUpService>();
        }
    }
}
