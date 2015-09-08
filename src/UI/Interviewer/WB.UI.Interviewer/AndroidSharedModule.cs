using Ninject.Modules;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Interviewer.Implementations.Services;

namespace WB.UI.Interviewer
{
    public class AndroidSharedModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<INetworkService>().To<AndroidNetworkService>().InSingletonScope();
            this.Bind<IRestService>().To<RestService>().WithConstructorArgument("restServicePointManager", _ => null);
        }
    }
}
