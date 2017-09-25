using System;
using System.Collections.Concurrent;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Ninject;
using Ninject;
using Ninject.Modules;
using Prometheus.Advanced;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Versions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Headquarters.API.WebInterview.Pipeline;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class WebInterviewNinjectModule : NinjectModule
    {
        public override void Load()
        {
            GlobalHost.DependencyResolver = new NinjectDependencyResolver(this.Kernel);
            var pipiline = GlobalHost.DependencyResolver.Resolve<IHubPipeline>();

            pipiline.AddModule(new SignalrErrorHandler());
            pipiline.AddModule(new PlainSignalRTransactionManager());
            pipiline.AddModule(new WebInterviewAllowedModule());
            pipiline.AddModule(new WebInterviewStateManager(GlobalHost.DependencyResolver.Resolve<IProductVersion>(), GlobalHost.DependencyResolver.Resolve<IStatefulInterviewRepository>()));
            pipiline.AddModule(new WebInterviewConnectionsCounter());

            this.Bind<IWebInterviewNotificationService>().To<WebInterviewLazyNotificationService>().InSingletonScope();
            this.Bind<IConnectionLimiter>().To<ConnectionLimiter>();

            DefaultCollectorRegistry.Instance.RegisterOnDemandCollectors(new IOnDemandCollector[]
            {
                new DotNetStatsCollector ()
            });

            this.Bind<IJavaScriptMinifier>().ToConstant(new SignalRHubMinifier());

            this.Bind<IHubContext>()
                .ToMethod(context => GlobalHost.ConnectionManager.GetHubContext<WebInterview>())
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