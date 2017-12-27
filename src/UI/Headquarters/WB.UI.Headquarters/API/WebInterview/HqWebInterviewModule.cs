using System;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Headquarters.API.WebInterview.Pipeline;
using WB.UI.Headquarters.API.WebInterview.Services;

namespace WB.UI.Headquarters.API.WebInterview
{
    public class HqWebInterviewModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<IStatefullInterviewSearcher, StatefullInterviewSearcher>();

            foreach (var type in HubPipelineModules)
            {
                registry.BindAsSingleton(typeof(IHubPipelineModule), type);
            }
        }

        private static readonly Type[] HubPipelineModules =
        {
            typeof(PlainSignalRTransactionManager),
            typeof(InterviewAuthorizationModule),
            typeof(WebInterviewStateManager),
        };
    }
}