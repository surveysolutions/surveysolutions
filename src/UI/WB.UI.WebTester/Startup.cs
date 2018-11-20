using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web.Hosting;
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
using StackExchange.Exceptional.Stores;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Enumerator.Native.WebInterview;
using WB.UI.WebTester.Hub;
using WB.UI.WebTester.Infrastructure;
using WB.UI.WebTester.Services;
using WB.UI.WebTester.Services.Implementation;

[assembly: OwinStartup(typeof(WB.UI.WebTester.Startup))]

namespace WB.UI.WebTester
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            EnsureJsonStorageForErrorsExists();
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info($"Application started {FileVersionInfo.GetVersionInfo(typeof(Startup).Assembly.Location).ProductVersion}");
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

            InScopeExecutor.Init(new NoScopeInScopeExecutor(container));
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));
            WebInterviewModule.Configure(app, WebTesterModule.HubPipelineModules);
            
            WebApiConfig.Register(config);

            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);
            app.UseWebApi(config);
            MetricsService.Start(logger);

            evictionService = ServiceLocator.Current.GetInstance<EvictionService>();
        }

        private EvictionService evictionService;

        private void EnsureJsonStorageForErrorsExists()
        {
            if (StackExchange.Exceptional.Exceptional.Settings.DefaultStore is JSONErrorStore exceptionalConfig)
            {
                var jsonStorePath = exceptionalConfig.Settings.Path;
                var jsonStorePathAbsolute = HostingEnvironment.MapPath(jsonStorePath);

                if (!Directory.Exists(jsonStorePathAbsolute))
                {
                    Directory.CreateDirectory(jsonStorePathAbsolute);
                }
            }
        }

    }
}
