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
            var pipiline = GlobalHost.DependencyResolver.Resolve<IHubPipeline>();

            pipiline.AddModule(new SignalrErrorHandler());
            pipiline.AddModule(new PlainSignalRTransactionManager());
            pipiline.AddModule(new WebInterviewAllowedModule());
            pipiline.AddModule(new WebInterviewStateManager());
            pipiline.AddModule(new WebInterviewConnectionsCounter());

            this.Bind<IWebInterviewNotificationService>().To<WebInterviewNotificationService>();
            this.Bind<IConnectionLimiter>().To<ConnectionLimiter>();

            this.Bind<IHubContext>()
                .ToMethod(context => GlobalHost.ConnectionManager.GetHubContext<WebInterview>())
                .InSingletonScope()
                .Named(@"WebInterview");
        }
    }
}