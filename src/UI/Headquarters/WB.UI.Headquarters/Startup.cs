using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Ninject;
using Microsoft.Owin;
using Owin;
using WB.UI.Headquarters;

[assembly: OwinStartup(typeof(Startup))]
namespace WB.UI.Headquarters
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalHost.DependencyResolver = new NinjectDependencyResolver(new Ninject.Web.Common.Bootstrapper().Kernel);
            app.MapSignalR(new HubConfiguration {EnableDetailedErrors = true});
        }
    }
}