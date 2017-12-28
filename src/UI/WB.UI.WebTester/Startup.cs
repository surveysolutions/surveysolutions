using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Ninject;
using Ninject.Web.Common.OwinHost;
using Owin;
using WB.UI.Shared.Web.Modules;

[assembly: OwinStartup(typeof(WB.UI.WebTester.Startup))]

namespace WB.UI.WebTester
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var kernel = ConfigureNinject(app);

            app.MapSignalR(new HubConfiguration {EnableDetailedErrors = true});
        }

        private IKernel ConfigureNinject(IAppBuilder app)
        {
            var kernel = NinjectConfig.CreateKernel();
            app.UseNinjectMiddleware(() => kernel);
            return kernel;
        }
    }
}
