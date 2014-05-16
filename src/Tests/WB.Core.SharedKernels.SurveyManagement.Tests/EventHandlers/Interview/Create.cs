using System;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview
{
    internal static class Create
    {
        private static IPublishedEvent<T> ToPublishedEvent<T>(T @event)
            where T : class
        {
            return ToPublishedEvent<T>(@event, Guid.NewGuid());
        }

        private static IPublishedEvent<T> ToPublishedEvent<T>(T @event, Guid eventSourceId)
            where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent
                => publishedEvent.Payload == @event &&
                   publishedEvent.EventSourceId == eventSourceId);
        }

        public static IPublishedEvent<InterviewCreated> InterviewCreatedEvent(string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion));
        }

        public static IPublishedEvent<InterviewFromPreloadedDataCreated> InterviewFromPreloadedDataCreatedEvent(string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewFromPreloadedDataCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion));
        }

        public static IPublishedEvent<InterviewOnClientCreated> InterviewOnClientCreatedEvent(string userId = null,
            string questionnaireId = null, long questionnaireVersion = 0)
        {
            return
                ToPublishedEvent(new InterviewOnClientCreated(userId: GetGuidIdByStringId(userId),
                    questionnaireId: GetGuidIdByStringId(questionnaireId), questionnaireVersion: questionnaireVersion));
        }

        public static IPublishedEvent<SupervisorAssigned> SupervisorAssignedEvent(string userId = null,
            string supervisorId = null)
        {
            return
                ToPublishedEvent(new SupervisorAssigned(userId: GetGuidIdByStringId(userId),
                    supervisorId: GetGuidIdByStringId(supervisorId)));
        }

        public static IPublishedEvent<InterviewerAssigned> InterviewerAssignedEvent(string userId = null,
            string interviewerId = null)
        {
            return
                ToPublishedEvent(new InterviewerAssigned(userId: GetGuidIdByStringId(userId),
                    interviewerId: GetGuidIdByStringId(interviewerId)));
        }

        public static IPublishedEvent<InterviewDeleted> InterviewDeletedEvent(string userId = null)
        {
            return ToPublishedEvent(new InterviewDeleted(userId: GetGuidIdByStringId(userId)));
        }

        public static IPublishedEvent<InterviewRestored> InterviewRestoredEvent(string userId = null)
        {
            return ToPublishedEvent(new InterviewRestored(userId: GetGuidIdByStringId(userId)));
        }

        public static IPublishedEvent<InterviewRestarted> InterviewRestartedEvent(string userId = null)
        {
            return ToPublishedEvent(new InterviewRestarted(userId: GetGuidIdByStringId(userId)));
        }

        public static IPublishedEvent<InterviewCompleted> InterviewCompletedEvent(string userId = null)
        {
            return ToPublishedEvent(new InterviewCompleted(userId: GetGuidIdByStringId(userId)));
        }

        public static IPublishedEvent<InterviewRejected> InterviewRejectedEvent(string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRejected(userId: GetGuidIdByStringId(userId), comment: comment));
        }

        public static IPublishedEvent<InterviewApproved> InterviewApprovedEvent(string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewApproved(userId: GetGuidIdByStringId(userId), comment: comment));
        }

        public static IPublishedEvent<InterviewRejectedByHQ> InterviewRejectedByHQEvent(string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewRejectedByHQ(userId: GetGuidIdByStringId(userId), comment: comment));
        }

        public static IPublishedEvent<InterviewApprovedByHQ> InterviewApprovedByHQEvent(string userId = null, string comment = null)
        {
            return ToPublishedEvent(new InterviewApprovedByHQ(userId: GetGuidIdByStringId(userId), comment: comment));
        }

        private static Guid GetGuidIdByStringId(string stringId)
        {
            return string.IsNullOrEmpty(stringId) ? Guid.NewGuid() : Guid.Parse(stringId);
        }
    }
}
