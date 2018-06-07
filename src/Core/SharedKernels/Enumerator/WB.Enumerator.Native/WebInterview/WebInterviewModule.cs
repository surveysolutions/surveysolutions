using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Owin;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Enumerator.Native.WebInterview.Services;

namespace WB.Enumerator.Native.WebInterview
{
    public class WebInterviewModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IConnectionsMonitor, ConnectionsMonitor>();
            registry.BindAsSingleton<IWebInterviewNotificationService, WebInterviewLazyNotificationService>();
            registry.BindAsSingletonWithConstructorArgument<IConnectionLimiter, ConnectionLimiter>("connectionsLimit",
                ConfigurationManager.AppSettings["MaxWebInterviewsCount"].ToInt(100));

            registry.BindToConstant<IJavaScriptMinifier>(() => new SignalRHubMinifier());

            registry.BindToMethodInSingletonScope<IWebInterviewInvoker>(_ =>
            {
                // Ninject calls this method before container innitialization. Just make sure that we can handle this in AutoFac
                var lazyClients = new Lazy<IHubConnectionContext<dynamic>>(
                    () => GlobalHost.ConnectionManager.GetHubContext("interview").Clients);

                return new WebInterviewInvoker(lazyClients);
            });
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }


        public static void Configure(IAppBuilder app, Type[] pipelineModules)
        {
            var resolver = GlobalHost.DependencyResolver;
            var pipeline = resolver.Resolve<IHubPipeline>() ?? throw new ArgumentNullException("resolver.Resolve<IHubPipeline>()");

            foreach (var moduleType in pipelineModules)
            {
                var module = resolver.GetService(moduleType) as IHubPipelineModule;
                if (module == null) throw new ArgumentNullException(nameof(module), $"Tried to resolve type {moduleType}");
                pipeline.AddModule(module);
            }

            (resolver.GetService(typeof(IConnectionsMonitor)) as IConnectionsMonitor)?.StartMonitoring();

            app.MapSignalR(new HubConfiguration { EnableDetailedErrors = true });
        }

        internal class SignalRHubMinifier : IJavaScriptMinifier
        {
            readonly ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();

            public string Minify(string source)
            {
                return this.cache.GetOrAdd(source, s => new Minifier().MinifyJavaScript(source, new CodeSettings
                {
                    PreserveImportantComments = false
                }));
            }
        }
    }
}
