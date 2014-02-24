using WB.Core.GenericSubdomains.Rest;
using Ninject.Modules;
namespace WB.Core.GenericSubdomains.Rest.Android
{
    public class RestAndroidModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IRestServiceWrapperFactory>().To<AndroidRestServiceWrapperFactory>();
            this.Bind<INetworkService>().To<AndroidNetworkService>();
        }
    }
}