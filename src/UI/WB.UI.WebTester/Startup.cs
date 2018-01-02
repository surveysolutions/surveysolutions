using System.Reflection;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.WebTester.Hub;

[assembly: OwinStartup(typeof(WB.UI.WebTester.Startup))]

namespace WB.UI.WebTester
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ContainerBuilder builder = AutofacConfig.CreateKernel();
            
            var hubConfiguration = new HubConfiguration {EnableDetailedErrors = true};
            builder.RegisterHubs(Assembly.GetAssembly(typeof(WebInterviewHub)));
            builder.RegisterControllers(typeof(MvcApplication).Assembly);

            var container = builder.Build();
            GlobalHost.DependencyResolver = new Autofac.Integration.SignalR.AutofacDependencyResolver(container);
            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));
            app.UseAutofacMiddleware(container);
            app.MapSignalR(hubConfiguration);
        }
    }
}
