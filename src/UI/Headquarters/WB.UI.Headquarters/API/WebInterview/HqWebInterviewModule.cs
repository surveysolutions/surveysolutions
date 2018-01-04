﻿using System;
using Microsoft.AspNet.SignalR.Hubs;
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

            foreach (var type in HubPipelineModules)
            {
                registry.BindAsSingleton(typeof(IHubPipelineModule), type);
            }
        }

        public static Type[] HubPipelineModules => new[]
        {
            typeof(SignalrErrorHandler),
            typeof(WebInterviewStateManager),
            typeof(PlainSignalRTransactionManager),
            typeof(InterviewAuthorizationModule),
            typeof(WebInterviewStateManager),
            typeof(WebInterviewConnectionsCounter)
        };
    }
}