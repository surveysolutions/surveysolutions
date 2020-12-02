using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Implementation.EventDispatcher;
using WB.Core.Infrastructure.Modularity;
using WB.Enumerator.Native.WebInterview.LifeCycle;
using WB.Enumerator.Native.WebInterview.Pipeline;
using WB.Enumerator.Native.WebInterview.Services;

namespace WB.Enumerator.Native.WebInterview
{
    public class WebInterviewModule : IModule, IInitModule
    {
        private readonly IConfiguration configuration;

        public WebInterviewModule(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Load(IIocRegistry registry)
        {
            registry.Bind<IWebInterviewNotificationService, WebInterviewNotificationService>();

            registry.Bind<InterviewLifecycleEventHandler>();
            registry.Bind<IWebInterviewInvoker, WebInterviewInvoker>();
            registry.Bind<IPipelineModule, WebInterviewConnectionsCounter>();
            registry.Bind<IPipelineModule, StatefulInterviewCachePinModule>();
            registry.Bind<IWebInterviewNotificationBuilder, WebInterviewNotificationBuilder>();
        }

        public Task Init(IServiceLocator serviceLocator, UnderConstructionInfo status)
        {
            var registry = serviceLocator.GetInstance<IDenormalizerRegistry>();
            registry.RegisterFunctional<InterviewLifecycleEventHandler>();

            return Task.CompletedTask;
        }
    }
}
