using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.UI.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class NetworkModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<INetworkService>().To<NetworkService>().InSingletonScope();
            this.Bind<IRestService>().To<RestService>().InSingletonScope();
            this.Bind<IRestServiceSettings>().To<RestServiceSettings>().InSingletonScope();
        }
    }
}