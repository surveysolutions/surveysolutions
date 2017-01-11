using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Services.WebInterview;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Headquarters.EventHandler
{
    internal class InterviewLifecycleEventHandler :
        IEventHandler,
        IEventHandler<AnswersDeclaredInvalid>,
        IEventHandler<AnswersDeclaredValid>
    {
        private readonly IWebInterviewNotificationService webInterviewNotificationService;

        public InterviewLifecycleEventHandler(IWebInterviewNotificationService webInterviewNotificationService)
        {
            this.webInterviewNotificationService = webInterviewNotificationService;
        }

        public void Handle(IPublishedEvent<AnswersDeclaredInvalid> @event)
        {
            this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId, @event.Payload.Questions);
        }

        public void Handle(IPublishedEvent<AnswersDeclaredValid> @event)
        {
            this.webInterviewNotificationService.RefreshEntities(@event.EventSourceId, @event.Payload.Questions);
        }

        public string Name { get; } = nameof(InterviewEventHandlerFunctional);
        public object[] Readers { get; } = new object[0];
        public object[] Writers { get; } = new object[0];
    }
}