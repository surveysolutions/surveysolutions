using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.Android.Implementation.Services;
using WB.Core.Infrastructure.Android.Implementation.Services.Network;
using WB.Core.Infrastructure.Android.Implementation.Services.Rest;
using WB.Core.Infrastructure.Android.Implementation.Services.Settings;

namespace WB.Core.Infrastructure.Android
{
    public class NetworkModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<INetworkService>().To<NetworkService>().InSingletonScope();
            this.Bind<IRestService>().To<RestService>().InSingletonScope();

            var applicationSettings = this.Kernel.Get<ApplicationSettings>();

            this.Bind<RestServiceSettings>().ToConstant(new RestServiceSettings()
            {
                AcceptUnsignedSslCertificate = applicationSettings.AcceptUnsignedSslCertificate,
                BufferSize = applicationSettings.BufferSize,
                Endpoint = applicationSettings.DesignerEndpoint,
                Timeout = applicationSettings.HttpResponseTimeout
            });
        }
    }
}