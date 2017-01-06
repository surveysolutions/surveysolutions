using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Ninject;
using Microsoft.Owin;
using Owin;
using WB.UI.Headquarters;
using WB.UI.Headquarters.API.WebInterview;

[assembly: OwinStartup(typeof(Startup))]
namespace WB.UI.Headquarters
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR(new HubConfiguration
            {
                EnableDetailedErrors = true,
                Resolver = new NinjectDependencyResolver(new Ninject.Web.Common.Bootstrapper().Kernel)
            });
        }
    }
}