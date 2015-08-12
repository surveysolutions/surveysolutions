using Ninject;
using Ninject.Modules;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.UI.Capi.Infrastructure.Internals.Security;
using WB.UI.Capi.Infrastructure.Internals.Settings;

namespace WB.UI.Capi.Infrastructure
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