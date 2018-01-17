using System.Diagnostics;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using NLog;
using Owin;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Enumerator.Native.WebInterview;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.WebTester.Hub;

[assembly: OwinStartup(typeof(WB.UI.WebTester.Startup))]

namespace WB.UI.WebTester
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            LogManager.GetCurrentClassLogger().Info($"Application started {FileVersionInfo.GetVersionInfo(typeof(Startup).Assembly.Location).ProductVersion}");
            ConfigurationSource.Init();

            ContainerBuilder builder = AutofacConfig.CreateKernel();
            var config = new HttpConfiguration();

            builder.RegisterHubs(Assembly.GetAssembly(typeof(WebInterviewHub)));
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.RegisterApiControllers(typeof(MvcApplication).Assembly);

            var container = builder.Build();
            GlobalHost.DependencyResolver = new Autofac.Integration.SignalR.AutofacDependencyResolver(container);
            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));
            WebInterviewModule.Configure(app, WebTesterModule.HubPipelineModules);
            
            WebApiConfig.Register(config);

            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);
            app.UseWebApi(config);
        }
    }
}
