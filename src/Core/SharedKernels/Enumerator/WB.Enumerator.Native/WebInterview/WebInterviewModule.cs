using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Enumerator.Native.WebInterview.Services;

namespace WB.Enumerator.Native.WebInterview
{
    public class WebInterviewModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IConnectionsMonitor, ConnectionsMonitor>();
            registry.Bind<IWebInterviewNotificationService, WebInterviewLazyNotificationService>();
            registry.BindAsSingletonWithConstructorArgument<IConnectionLimiter, ConnectionLimiter>("connectionsLimit",
                ConfigurationManager.AppSettings["MaxWebInterviewsCount"].ToInt(100));

            registry.BindInPerLifetimeScope<InterviewLifecycleEventHandler, InterviewLifecycleEventHandler>();
            //registry.BindToConstant<IJavaScriptMinifier>(() => new SignalRHubMinifier());

            registry.BindToMethodInSingletonScope<IWebInterviewInvoker>(_ =>
            {
                // Ninject calls this method before container innitialization. Just make sure that we can handle this in AutoFac
//                var lazyClients = new Lazy<IHubClients<dynamic>>(
//                    () => GlobalHost.ConnectionManager.GetHubContext("interview").Clients);
                var lazyClients = new Lazy<IHubClients>(() => _.Resolve<IHubClients>());

                return new WebInterviewInvoker(lazyClients);
            });
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            var registry = serviceLocator.GetInstance<IDenormalizerRegistry>();
            registry.Register<InterviewLifecycleEventHandler>();

            return Task.CompletedTask;
        }

        /*internal class SignalRHubMinifier : IJavaScriptMinifier
        {
            readonly ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();

            public string Minify(string source)
            {
                return this.cache.GetOrAdd(source, s => new Minifier().MinifyJavaScript(source, new CodeSettings
                {
                    PreserveImportantComments = false
                }));
            }
        }*/
    }
}
