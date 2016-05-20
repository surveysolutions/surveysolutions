using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Unit.TestFactories
{
    internal class PublishedEventFactory
    {
        public IPublishedEvent<InterviewDeleted> InterviewDeleted(Guid? interviewId = null)
            => Create.Event.InterviewDeleted().ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewHardDeleted> InterviewHardDeleted(Guid? interviewId = null)
            => Create.Event.InterviewHardDeleted().ToPublishedEvent(eventSourceId: interviewId);


        public IPublishedEvent<InterviewStatusChanged> InterviewStatusChanged(
            Guid interviewId, InterviewStatus status, string comment = "hello", Guid? eventId = null)
            => Create.Event.InterviewStatusChanged(status, comment).ToPublishedEvent(eventSourceId: interviewId, eventId: eventId);
    }
}