using System.Collections.Concurrent;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Headquarters.API.WebInterview.Pipeline;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class WebInterviewModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.BindAsSingleton<IConnectionsMonitor, ConnectionsMonitor>();
            registry.BindAsSingleton<IWebInterviewNotificationService, WebInterviewLazyNotificationService>();
            registry.Bind<IConnectionLimiter, ConnectionLimiter>();
            registry.Bind<IStatefullInterviewSearcher, StatefullInterviewSearcher>();
            registry.Bind<IWebInterviewInterviewEntityFactory, WebInterviewInterviewEntityFactory>();

            registry.BindToConstant<IJavaScriptMinifier>(() => new SignalRHubMinifier());

            registry.BindAsSingleton<IHubPipelineModule, SignalrErrorHandler>();
            registry.BindAsSingleton<IHubPipelineModule, PlainSignalRTransactionManager>();
            registry.BindAsSingleton<IHubPipelineModule, InterviewAuthorizationModule>();
            registry.BindAsSingleton<IHubPipelineModule, WebInterviewStateManager>();
            registry.BindAsSingleton<IHubPipelineModule, WebInterviewConnectionsCounter>();

            registry.BindToMethodInSingletonScope<IWebInterviewInvoker>(_ =>
            {
                var hub = GlobalHost.ConnectionManager.GetHubContext<WebInterview>();
                return new WebInterviewInvoker(hub.Clients);
            });
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