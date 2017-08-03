using Ninject.Modules;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.UI.Interviewer.Services;
using WB.UI.Shared.Enumerator.CustomServices;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.UI.Interviewer
{
    public class AndroidSharedModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<INetworkService>().To<AndroidNetworkService>();
            this.Bind<IHttpClientFactory>().To<ModernHttpClientFactory>();
            this.Bind<IRestService>().To<RestService>().WithConstructorArgument("restServicePointManager", _ => null);
            this.Bind<IInterviewUniqueKeyGenerator>().To<InterviewerInterviewUniqueKeyGenerator>();
        }
    }
}
