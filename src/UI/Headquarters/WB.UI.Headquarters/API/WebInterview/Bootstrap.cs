using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Ninject;
using Owin;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class Bootstrap
    {
        public static void Configure(IAppBuilder app, IKernel kernel)
        {
            var pipeline = GlobalHost.DependencyResolver.Resolve<IHubPipeline>();
            var pipelineModules = kernel.GetAll<IHubPipelineModule>();

            foreach (var module in pipelineModules)
            {
                pipeline.AddModule(module);
            }

            kernel.Get<IConnectionsMonitor>().StartMonitoring();

            app.MapSignalR(new HubConfiguration { EnableDetailedErrors = true });
        }
    }
}