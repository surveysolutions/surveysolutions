using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.TestFactories
{
    internal class PublishedEventFactory
    {
        public IPublishedEvent<InterviewApprovedByHQ> InterviewApprovedByHQEvent(
            Guid? interviewId = null, string userId = null, string comment = null)
            => new InterviewApprovedByHQ(
                ToGuid(userId) ?? Guid.NewGuid(), comment)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewApproved> InterviewApprovedEvent(Guid? interviewId = null, string userId = null, string comment = null)
            => new InterviewApproved(ToGuid(userId) ?? Guid.NewGuid(), comment, DateTime.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewCompleted> InterviewCompletedEvent(
            Guid? interviewId = null, string userId = null, string comment = null, Guid? eventId = null)
            => new InterviewCompleted(ToGuid(userId) ?? Guid.NewGuid(), DateTime.Now, comment)
                .ToPublishedEvent(eventSourceId: interviewId, eventId: eventId);

        public IPublishedEvent<InterviewCreated> InterviewCreatedEvent(
            Guid? interviewId = null, string userId = null, string questionnaireId = null, long questionnaireVersion = 0)
            => new InterviewCreated(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(questionnaireId) ?? Guid.NewGuid(), questionnaireVersion)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewDeleted> InterviewDeleted(Guid? interviewId = null)
            => Create.Event.InterviewDeleted()
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewDeleted> InterviewDeletedEvent(string userId = null, string origin = null, Guid? interviewId = null)
            => new InterviewDeleted(ToGuid(userId) ?? Guid.NewGuid())
                .ToPublishedEvent(origin: origin, eventSourceId: interviewId);

        public IPublishedEvent<InterviewerAssigned> InterviewerAssignedEvent(
            Guid? interviewId = null, string userId = null, string interviewerId = null)
            => new InterviewerAssigned(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(interviewerId) ?? Guid.NewGuid(), DateTime.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewFromPreloadedDataCreated> InterviewFromPreloadedDataCreatedEvent(
            Guid? interviewId = null, string userId = null, string questionnaireId = null, long questionnaireVersion = 0)
            => new InterviewFromPreloadedDataCreated(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(questionnaireId) ?? Guid.NewGuid(), questionnaireVersion)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewHardDeleted> InterviewHardDeleted(Guid? interviewId = null)
            => Create.Event.InterviewHardDeleted()
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewHardDeleted> InterviewHardDeletedEvent(string userId = null, Guid? interviewId = null)
            => new InterviewHardDeleted(ToGuid(userId) ?? Guid.NewGuid())
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewOnClientCreated> InterviewOnClientCreatedEvent(
            Guid? interviewId = null, string userId = null, string questionnaireId = null, long questionnaireVersion = 0)
            => new InterviewOnClientCreated(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(questionnaireId) ?? Guid.NewGuid(), questionnaireVersion)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewRejectedByHQ> InterviewRejectedByHQEvent(Guid? interviewId = null, string userId = null, string comment = null)
            => new InterviewRejectedByHQ(ToGuid(userId) ?? Guid.NewGuid(), comment)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewRejected> InterviewRejectedEvent(Guid? interviewId = null, string userId = null, string comment = null)
            => new InterviewRejected(ToGuid(userId) ?? Guid.NewGuid(), comment, DateTime.Now)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewRestarted> InterviewRestartedEvent(Guid? interviewId = null, string userId = null, string comment = null)
            => new InterviewRestarted(ToGuid(userId) ?? Guid.NewGuid(), DateTime.Now, comment)
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<InterviewRestored> InterviewRestoredEvent(Guid? interviewId = null, string userId = null, string origin = null)
            => new InterviewRestored(ToGuid(userId) ?? Guid.NewGuid())
                .ToPublishedEvent(origin: origin, eventSourceId: interviewId);

        public IPublishedEvent<InterviewStatusChanged> InterviewStatusChanged(
            Guid interviewId, InterviewStatus status, string comment = "hello", Guid? eventId = null)
            => Create.Event.InterviewStatusChanged(status, comment)
                .ToPublishedEvent(eventSourceId: interviewId, eventId: eventId);

        public IPublishedEvent<InterviewStatusChanged> InterviewStatusChangedEvent(
            InterviewStatus status, string comment = null, Guid? interviewId = null)
            => new InterviewStatusChanged(status, comment)
                .ToPublishedEvent(eventSourceId: interviewId ?? Guid.NewGuid());

        public IPublishedEvent<QuestionCloned> QuestionClonedEvent(string questionId = null,
            string parentGroupId = null, string questionVariable = null, string questionTitle = null,
            QuestionType questionType = QuestionType.Text, string questionConditionExpression = null,
            string sourceQuestionId = null, IList<ValidationCondition> validationConditions = null,
            bool hideIfDisabled = false, QuestionProperties properties = null)
            => new QuestionCloned(
                publicKey: ToGuid(questionId) ?? Guid.NewGuid(),
                groupPublicKey: ToGuid(parentGroupId) ?? Guid.NewGuid(),
                stataExportCaption: questionVariable,
                questionText: questionTitle,
                questionType: questionType,
                conditionExpression: questionConditionExpression,
                hideIfDisabled: hideIfDisabled,
                sourceQuestionId: ToGuid(sourceQuestionId) ?? Guid.NewGuid(),
                targetIndex: 0,
                featured: false,
                instructions: null,
                properties: properties ?? new QuestionProperties(false, false),
                responsibleId: Guid.NewGuid(),
                capital: false,
                questionScope: QuestionScope.Interviewer,
                variableLabel: null,
                validationExpression: null,
                validationMessage: null,
                answerOrder: null,
                answers: null,
                linkedToQuestionId: null,
                linkedToRosterId: null,
                isInteger: null,
                areAnswersOrdered: null,
                yesNoView: null,
                mask: null,
                maxAllowedAnswers: null,
                isFilteredCombobox: null,
                cascadeFromQuestionId: null,
                sourceQuestionnaireId: null,
                maxAnswerCount: null,
                countOfDecimalPlaces: null,
                validationConditions: validationConditions,
                linkedFilterExpression: null)
                .ToPublishedEvent();

        public IPublishedEvent<SupervisorAssigned> SupervisorAssignedEvent(Guid? interviewId = null, string userId = null,
            string supervisorId = null)
            => new SupervisorAssigned(ToGuid(userId) ?? Guid.NewGuid(), ToGuid(supervisorId) ?? Guid.NewGuid())
                .ToPublishedEvent(eventSourceId: interviewId);

        public IPublishedEvent<SynchronizationMetadataApplied> SynchronizationMetadataAppliedEvent(string userId = null,
            InterviewStatus status = InterviewStatus.Created, string questionnaireId = null,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta = null, bool createdOnClient = false)
            => new SynchronizationMetadataApplied(
                userId: ToGuid(userId) ?? Guid.NewGuid(),
                status: status,
                questionnaireId: ToGuid(questionnaireId) ?? Guid.NewGuid(),
                questionnaireVersion: 1,
                featuredQuestionsMeta: featuredQuestionsMeta,
                createdOnClient: createdOnClient,
                comments: null,
                rejectedDateTime: null,
                interviewerAssignedDateTime: null)
                .ToPublishedEvent();

        public IPublishedEvent<TextQuestionAnswered> TextQuestionAnsweredEvent(Guid? interviewId = null, string userId = null)
            => new TextQuestionAnswered(ToGuid(userId) ?? Guid.NewGuid(), Guid.NewGuid(), new decimal[0], DateTime.Now, "tttt")
                .ToPublishedEvent();

        private static Guid? ToGuid(string stringGuid)
            => string.IsNullOrEmpty(stringGuid)
                ? null as Guid?
                : Guid.Parse(stringGuid);

        public IPublishedEvent<UnapprovedByHeadquarters> UnapprovedByHeadquartersEvent(
            Guid? interviewId = null, string userId = null, string comment = null)
            => new UnapprovedByHeadquarters(ToGuid(userId) ?? Guid.NewGuid(), comment)
                .ToPublishedEvent(eventSourceId: interviewId);
    }
}