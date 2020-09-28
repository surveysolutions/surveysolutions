#nullable enable

using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.Headquarters.CompletedEmails
{
    public class CompletedEmailDenormalizer:
        BaseDenormalizer,
        IEventHandler<InterviewCompleted>
    {
        private readonly ICompletedEmailsQueue completedEmailsQueue;

        public CompletedEmailDenormalizer(ICompletedEmailsQueue completedEmailsQueue)
        {
            this.completedEmailsQueue = completedEmailsQueue;
        }

        public void Handle(IPublishedEvent<InterviewCompleted> @event)
        {
            completedEmailsQueue.Add(@event.EventSourceId);
        }
    }
}