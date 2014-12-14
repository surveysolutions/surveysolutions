using Ninject.Modules;
using WB.Core.SharedKernel.Utils.Implementation.Services;
using WB.Core.SharedKernel.Utils.Services;
using WB.Core.SharedKernel.Utils.Services.Rest;
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
