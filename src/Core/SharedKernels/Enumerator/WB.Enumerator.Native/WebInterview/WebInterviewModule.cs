using System;
using System.Collections.Concurrent;
using System.Configuration;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Owin;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.UI.Headquarters.API.WebInterview
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

            //foreach (var type in HubPipelineModules)
            //{
            //    registry.BindAsSingleton(typeof(IHubPipelineModule), type);
            //}

            registry.BindToMethodInSingletonScope<IWebInterviewInvoker>(_ =>
            {
                // Ninject calls this method before container innitialization. Just make sure that we can handle this in AutoFac
                var lazyClients = new Lazy<IHubConnectionContext<dynamic>>(
                    () => GlobalHost.ConnectionManager.GetHubContext<WebInterview>().Clients);

                return new WebInterviewInvoker(lazyClients);
            });
        }

        //private static readonly Type[] HubPipelineModules =
        //{
        //    typeof(SignalrErrorHandler),
        //    typeof(WebInterviewConnectionsCounter)
        //};

        public static void Configure(IAppBuilder app, Type[] pipelineModules)
        {
            var resolver = GlobalHost.DependencyResolver;
            var pipeline = resolver.Resolve<IHubPipeline>();

            foreach (var moduleType in pipelineModules)
            {
                var module = resolver.GetService(moduleType) as IHubPipelineModule;
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