using Microsoft.Owin;
using NConfig;
using Owin;

[assembly: OwinStartup(typeof(WB.UI.Headquarters.Startup))]
namespace WB.UI.Headquarters
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}