using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Pipeline;
using WB.UI.Headquarters.API.WebInterview.Pipeline;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class HqWebInterviewModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IWebInterviewInterviewEntityFactory, HqWebInterviewInterviewEntityFactory>();
            registry.Bind<IStatefullInterviewSearcher, StatefullInterviewSearcher>();
            registry.Bind<IInterviewOverviewService, InterviewOverviewService>();

            foreach (var type in HubPipelineModules)
            {
                registry.BindAsSingleton(typeof(IHubPipelineModule), type);
                registry.BindAsSingleton(type, type);
            }
        }

        public static Type[] HubPipelineModules => new[]
        {
            typeof(SignalrErrorHandler),
            typeof(HandlePauseEventPipelineModule),
            typeof(WebInterviewStateManager),
            typeof(PlainSignalRTransactionManager),
            typeof(WebInterviewConnectionsCounter)
        };

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            return Task.CompletedTask;
        }
    }
}
