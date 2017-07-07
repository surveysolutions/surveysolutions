using Ninject.Modules;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Shared.Enumerator.CustomServices;

namespace WB.UI.Interviewer
{
    public class AndroidSharedModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<INetworkService>().To<AndroidNetworkService>();
            this.Bind<IHttpClientFactory>().To<DefaultHttpClientFactory>();
            this.Bind<IRestService>().To<RestService>().WithConstructorArgument("restServicePointManager", _ => null);
        }
    }
}
