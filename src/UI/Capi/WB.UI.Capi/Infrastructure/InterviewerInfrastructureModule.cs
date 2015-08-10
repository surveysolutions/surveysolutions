using Ninject.Modules;
using WB.Core.SharedKernels.Enumerator;
using WB.UI.Capi.Infrastructure.Internals.Settings;

namespace WB.UI.Capi.Infrastructure
{
    public class InterviewerInfrastructureModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IEnumeratorSettings>().To<InterviewerSettings>();
        }
    }
}