using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.Android.Implementation.Services.Network;
using WB.Core.Infrastructure.Android.Implementation.Services.Rest;

namespace WB.Core.Infrastructure.Android
{
    public class NetworkModule : NinjectModule
    {
        private readonly RestServiceSettings settings;
        public NetworkModule(RestServiceSettings settings)
        {
            this.settings = settings;
        }

        public override void Load()
        {
            this.Bind<INetworkService>().To<NetworkService>().InSingletonScope();
            this.Bind<IRestService>().To<RestService>().InSingletonScope();
            this.Bind<RestServiceSettings>().ToConstant(this.settings);
        }
    }
}