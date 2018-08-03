using System;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Abc.TestFactories
{
    internal class PublishedEventFactory
    {
        public IPublishedEvent<InterviewApprovedByHQ> InterviewApprovedByHQ(
            Guid? interviewId = null, string userId = null, string comment = null, DateTimeOffset? originDate = null)
            => new InterviewApprovedByHQ(
                ToGuid(userId) ?? Guid.NewGuid(), comment, originDate ?? DateTimeOffset.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewApproved> InterviewApproved(Guid? interviewId = null, string userId = null, string comment = null, DateTimeOffset? originDate = null)
            => new InterviewApproved(ToGuid(userId) ?? Guid.NewGuid(), comment, originDate ?? DateTimeOffset.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewCompleted> InterviewCompleted(
            Guid? interviewId = null, string userId = null, string comment = null, Guid? eventId = null,
            DateTimeOffset? originDate = null)
            => new InterviewCompleted(ToGuid(userId) ?? Guid.NewGuid(), originDate ?? DateTimeOffset.Now, comment)
                .ToPublishedEvent(eventSourceId: interviewId, eventId: eventId);

        public IPublishedEvent<InterviewCreated> InterviewCreated(
            Guid? interviewId = null, string userId = null, 
            string questionnaireId = null,
            long questionnaireVersion = 0,
            DateTimeOffset? originDate = null,
            DateTime? eventTimeStamp = null)
            => new InterviewCreated(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(questionnaireId) ?? Guid.NewGuid(), questionnaireVersion, null, originDate: originDate ?? DateTimeOffset.Now)
                .ToPublishedEvent(eventSourceId: interviewId, eventTimeStamp: eventTimeStamp);

        public IPublishedEvent<InterviewDeleted> InterviewDeleted(string userId = null, string origin = null, Guid? interviewId = null, DateTimeOffset? originDate = null)
            => new InterviewDeleted(ToGuid(userId) ?? Guid.NewGuid(), originDate ?? DateTimeOffset.Now)
                .ToPublishedEvent(origin: origin, eventSourceId: interviewId);

        public IPublishedEvent<InterviewerAssigned> InterviewerAssigned(
            Guid? interviewId = null, string userId = null, string interviewerId = "", DateTimeOffset? originDate = null)
            => new InterviewerAssigned(ToGuid(userId) ?? Guid.NewGuid(),
                    interviewerId == null ? (Guid?) null : (ToGuid(interviewerId) ?? Guid.NewGuid()), originDate ?? DateTimeOffset.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewFromPreloadedDataCreated> InterviewFromPreloadedDataCreated(
            Guid? interviewId = null, string userId = null, string questionnaireId = null, long questionnaireVersion = 0, DateTimeOffset? originDate = null)
            => new InterviewFromPreloadedDataCreated(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(questionnaireId) ?? Guid.NewGuid(), questionnaireVersion, null, originDate ?? DateTimeOffset.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewHardDeleted> InterviewHardDeleted(string userId = null, Guid? interviewId = null)
            => Create.Event.InterviewHardDeleted(userId: ToGuid(userId))
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewOnClientCreated> InterviewOnClientCreated(
            Guid? interviewId = null, string userId = null, string questionnaireId = null, long questionnaireVersion = 0, DateTimeOffset? originDate = null)
            => new InterviewOnClientCreated(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(questionnaireId) ?? Guid.NewGuid(), questionnaireVersion, null, originDate ?? DateTimeOffset.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewRejectedByHQ> InterviewRejectedByHQ(Guid? interviewId = null, string userId = null, string comment = null, DateTimeOffset? originDate = null)
            => new InterviewRejectedByHQ(ToGuid(userId) ?? Guid.NewGuid(), comment, originDate ?? DateTimeOffset.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewRejected> InterviewRejected(Guid? interviewId = null, string userId = null, string comment = null)
            => new InterviewRejected(ToGuid(userId) ?? Guid.NewGuid(), comment, DateTime.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewRestarted> InterviewRestarted(Guid? interviewId = null, string userId = null, string comment = null)
            => new InterviewRestarted(ToGuid(userId) ?? Guid.NewGuid(), DateTime.Now, comment)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewRestored> InterviewRestored(Guid? interviewId = null, string userId = null, string origin = null, DateTimeOffset? originDate = null)
            => new InterviewRestored(ToGuid(userId) ?? Guid.NewGuid(), originDate ?? DateTimeOffset.Now)
                .ToPublishedEvent(origin: origin, eventSourceId: interviewId);

        public IPublishedEvent<InterviewStatusChanged> InterviewStatusChanged(
            Guid interviewId, InterviewStatus status, string comment = "hello", Guid? eventId = null, InterviewStatus? previousStatus = null)
            => Create.Event.InterviewStatusChanged(status, comment, previousStatus: previousStatus)
                .ToPublishedEvent(eventSourceId: interviewId, eventId: eventId);

        public IPublishedEvent<InterviewStatusChanged> InterviewStatusChanged(
            InterviewStatus status, string comment = null, Guid? interviewId = null)
            => Create.PublishedEvent.InterviewStatusChanged(interviewId ?? Guid.NewGuid(), status, comment: comment);

        public IPublishedEvent<SupervisorAssigned> SupervisorAssigned(Guid? interviewId = null, string userId = null,
            string supervisorId = null, DateTimeOffset? originDate = null)
            => new SupervisorAssigned(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(supervisorId) ?? Guid.NewGuid(), originDate ?? DateTimeOffset.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<SynchronizationMetadataApplied> SynchronizationMetadataApplied(string userId = null,
            InterviewStatus status = InterviewStatus.Created, string questionnaireId = null,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta = null, bool createdOnClient = false, DateTimeOffset? originDate = null)
            => new SynchronizationMetadataApplied(
                userId: ToGuid(userId) ?? Guid.NewGuid(),
                status: status,
                questionnaireId: ToGuid(questionnaireId) ?? Guid.NewGuid(),
                questionnaireVersion: 1,
                featuredQuestionsMeta: featuredQuestionsMeta,
                createdOnClient: createdOnClient,
                comments: null,
                rejectedDateTime: null,
                interviewerAssignedDateTime: null,
                originDate: originDate ?? DateTimeOffset.Now)
                .ToPublishedEvent();

        public IPublishedEvent<TextQuestionAnswered> TextQuestionAnswered(Guid? interviewId = null, string userId = null, DateTimeOffset? originDate = null)
            => new TextQuestionAnswered(ToGuid(userId) ?? Guid.NewGuid(), Guid.NewGuid(), new decimal[0], originDate ?? DateTimeOffset.Now, "tttt")
                .ToPublishedEvent();

        private static Guid? ToGuid(string stringGuid)
            => string.IsNullOrEmpty(stringGuid)
                ? null as Guid?
                : Guid.Parse(stringGuid);

        public IPublishedEvent<UnapprovedByHeadquarters> UnapprovedByHeadquarters(
            Guid? interviewId = null, string userId = null, string comment = null, DateTimeOffset? originDate = null)
            => new UnapprovedByHeadquarters(ToGuid(userId) ?? Guid.NewGuid(), comment, originDate ?? DateTimeOffset.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<RosterInstancesRemoved> RosterInstancesRemoved(Guid interviewId, DateTimeOffset? originDate = null, params RosterInstance[] instances)
            => new RosterInstancesRemoved(instances, originDate ?? DateTimeOffset.Now).ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<DateTimeQuestionAnswered> DateTimeQuestionAnswered(Guid? interviewId = null, Guid? userId = null, 
            Guid? questionId = null, DateTime? answer = null, DateTimeOffset? originDate = null)
            => new DateTimeQuestionAnswered(userId ?? Guid.NewGuid(), questionId ?? Guid.NewGuid(), new decimal[0], originDate ?? default(DateTimeOffset), answer ?? DateTime.Now)
                .ToPublishedEvent(eventSourceId: interviewId);
    }
}
