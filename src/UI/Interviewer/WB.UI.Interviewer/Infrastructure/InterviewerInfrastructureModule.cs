using Ninject.Modules;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Interviewer.Infrastructure.Internals.Security;
using WB.UI.Interviewer.Infrastructure.Internals.Settings;

namespace WB.UI.Interviewer.Infrastructure
{
    public class InterviewerInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IEnumeratorSettings>().To<InterviewerSettings>();
            this.Bind<IPrincipal>().To<InterviewerPrincipal>().InSingletonScope();
        }
    }
}