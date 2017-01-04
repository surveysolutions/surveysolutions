using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Ncqrs.Eventing.Sourcing;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview : Hub
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly ILiteEventBus liteEventBus;
        private readonly ICommandDeserializer commandDeserializer;
        private readonly ICommandService commandService;
        private readonly ILogger logger;

        private string interviewId => Context.ConnectionId;
        private IStatefulInterview CurrentInterview => this.statefulInterviewRepository.Get(this.interviewId);

        public WebInterview(
            IStatefulInterviewRepository statefulInterviewRepository, 
            ILiteEventBus liteEventBus,
            ICommandDeserializer commandDeserializer,
            ICommandService commandService,
            ILogger logger)
        {
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.liteEventBus = liteEventBus;
            this.commandDeserializer = commandDeserializer;
            this.commandService = commandService;
            this.logger = logger;
        }

        public void StartInterview()
            => this.liteEventBus.OnEventsPublished += LiteEventBus_OnEventsPublished;


        public override Task OnDisconnected(bool stopCalled)
        {
            // statefull interview can be removed from cache here
            this.liteEventBus.OnEventsPublished -= LiteEventBus_OnEventsPublished;

            return base.OnDisconnected(stopCalled);
        }

        private void LiteEventBus_OnEventsPublished(PublishedEventsArgs args)
        {
            var interviewEvents = args.PublishedEvents
                .Where(@evnt => @evnt.EventSourceId == this.CurrentInterview.Id)
                .Select(@evnt => @evnt.Payload)
                .ToArray();

            if (interviewEvents.Any()) this.Clients.Caller.applyEvents(interviewEvents);
        }
    }
}