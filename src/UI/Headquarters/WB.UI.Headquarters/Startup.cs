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
            app.MapSignalR();
        }
    }
}