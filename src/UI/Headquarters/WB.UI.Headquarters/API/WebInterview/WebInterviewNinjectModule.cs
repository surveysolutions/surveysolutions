using System.Collections.Concurrent;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.UI.Headquarters.API.WebInterview.Pipeline;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class WebInterviewNinjectModule : NinjectModule
    {
        public override void Load()
        {
            GlobalHost.DependencyResolver = new NinjectDependencyResolver(this.Kernel);
            
            this.Bind<IConnectionsMonitor>().To<ConnectionsMonitor>().InSingletonScope();
            this.Bind<IWebInterviewNotificationService>().To<WebInterviewLazyNotificationService>().InSingletonScope();
            this.Bind<IConnectionLimiter>().To<ConnectionLimiter>();
            this.Bind<IStatefullInterviewSearcher>().To<StatefullInterviewSearcher>();
            this.Bind<IWebInterviewInterviewEntityFactory>().To<WebInterviewInterviewEntityFactory>();
            
            this.Bind<IJavaScriptMinifier>().ToConstant(new SignalRHubMinifier());

            this.Bind<IHubPipelineModule>().To<SignalrErrorHandler>().InSingletonScope();
            this.Bind<IHubPipelineModule>().To<PlainSignalRTransactionManager>().InSingletonScope();
            this.Bind<IHubPipelineModule>().To<InterviewAuthorizationModule>().InSingletonScope();
            this.Bind<IHubPipelineModule>().To<WebInterviewStateManager>().InSingletonScope();
            this.Bind<IHubPipelineModule>().To<WebInterviewConnectionsCounter>().InSingletonScope();
            
            this.Bind<IHubContext>()
                .ToMethod(_ => GlobalHost.ConnectionManager.GetHubContext<WebInterview>())
                .InSingletonScope()
                .Named(@"WebInterview");
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