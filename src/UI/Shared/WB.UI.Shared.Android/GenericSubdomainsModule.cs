using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils.Implementation.Services.Rest;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.GenericSubdomains.Utils.Services.Rest;
using WB.UI.Shared.Android.Services;

namespace WB.UI.Shared.Android
{
    public class GenericSubdomainsModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<INetworkService>().To<AndroidNetworkService>().InSingletonScope();
            this.Bind<IRestService>().To<RestService>().InSingletonScope();
        }
    }
}
