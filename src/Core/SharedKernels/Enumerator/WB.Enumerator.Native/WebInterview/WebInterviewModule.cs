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
            
            registry.Bind<IWebInterviewNotificationService, WebInterviewLazyNotificationService>();
            
            registry.Bind<InterviewLifecycleEventHandler>();
            
            registry.BindAsSingleton<IWebInterviewInvoker, WebInterviewInvoker>();

        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            var registry = serviceLocator.GetInstance<IDenormalizerRegistry>();
            registry.Register<InterviewLifecycleEventHandler>();

            return Task.CompletedTask;
        }
    }
}
