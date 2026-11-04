using System;
using System.Linq;
using FluentAssertions.Common;
using Main.Core.Documents;
using Moq;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Tests.Abc;

using WB.UI.Headquarters.API.WebInterview.Services;
using WB.UI.Headquarters.Services.Impl;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    internal class when_getting_entity_details
    {
        private Mock<IAuthorizedUser> user;
        private HqWebInterviewInterviewEntityFactory service;
        private QuestionnaireDocument document;
        private StatefulInterview interview;
        private readonly Guid supervisorId = Id.gA;
        private readonly Guid interviewerId = Id.gB;
        private readonly Guid headquartersId = Id.gC;
        private readonly Guid commentedQuestionId = Id.g1;

        [SetUp]
        public void Setup()
        {
            user = new Mock<IAuthorizedUser>();
            service = Web.Create.Service.HqWebInterviewInterviewEntityFactory(user.Object);
            document = GetDocument();
            interview = Create.AggregateRoot.StatefulInterview(questionnaire: document, supervisorId: supervisorId, userId: interviewerId);
            interview.CommentAnswer(interviewerId, commentedQuestionId, RosterVector.Empty, DateTimeOffset.UtcNow, "test");
        }

        [Test]
        public void should_allow_resolving_comments_for_HQ()
        {
            user.Setup(x => x.IsHeadquarter).Returns(true);
           
            // Act
            var entity = service.GetEntityDetails(commentedQuestionId.FormatGuid(), interview, Create.Entity.PlainQuestionnaire(document), true);

            // Assert
            Assert.That(entity, Has.Property(nameof(InterviewTextQuestion.AllowResolveComments)).True);
        }

        [Test]
        public void should_allow_resolving_comments_for_Admin()
        {
            user.Setup(x => x.IsAdministrator).Returns(true);
           
            // Act
            var entity = service.GetEntityDetails(commentedQuestionId.FormatGuid(), interview, Create.Entity.PlainQuestionnaire(document), true);

            // Assert
            Assert.That(entity, Has.Property(nameof(InterviewTextQuestion.AllowResolveComments)).True);
        }

        [Test]
        public void should_allow_resolving_comments_for_Supervisor_when_no_HQ_comments_made()
        {
            user.Setup(x => x.IsSupervisor).Returns(true);
           
            // Act
            var entity = service.GetEntityDetails(commentedQuestionId.FormatGuid(), interview, Create.Entity.PlainQuestionnaire(document), true);

            // Assert
            Assert.That(entity, Has.Property(nameof(InterviewTextQuestion.AllowResolveComments)).True);
        }

        [Test]
        public void should_not_allow_resolving_comments_for_supervisor_when_HQ_comments_made()
        {
            user.Setup(x => x.IsSupervisor).Returns(true);
            interview.CommentAnswer(headquartersId, commentedQuestionId, RosterVector.Empty, DateTimeOffset.UtcNow,  "test1");
           
            // Act
            var entity = service.GetEntityDetails(commentedQuestionId.FormatGuid(), interview, Create.Entity.PlainQuestionnaire(document), true);

            // Assert
            Assert.That(entity, Has.Property(nameof(InterviewTextQuestion.AllowResolveComments)).False);
        }

        [Test]
        public void should_not_mark_answers_when_interview_has_not_been_rejected()
        {
            interview.AnswerTextQuestion(interviewerId, commentedQuestionId, RosterVector.Empty, DateTimeOffset.UtcNow, "answer");
            var questionnaire = Create.Entity.PlainQuestionnaire(document);

            var liveEntity = (InterviewTextQuestion)service.GetEntityDetails(
                commentedQuestionId.FormatGuid(), interview, questionnaire, true);
            Assert.That(liveEntity.IsChangedSinceReject, Is.False);

            var history = interview.GetUnCommittedChanges().Select(e => new CommittedEvent(
                Guid.Empty, null, e.EventIdentifier, e.EventSourceId, e.EventSequence, e.EventTimeStamp, null, e.Payload)).ToArray();
            var replayedInterview = Create.AggregateRoot.StatefulInterview(questionnaire: document, shouldBeInitialized: false);
            replayedInterview.InitializeFromHistory(interview.Id, history);

            var replayedEntity = (InterviewTextQuestion)service.GetEntityDetails(
                commentedQuestionId.FormatGuid(), replayedInterview, questionnaire, true);
            Assert.That(replayedEntity.IsChangedSinceReject, Is.False);
        }

        [Test]
        public void should_mark_question_changed_only_when_answered_after_last_reject()
        {
            var t0 = DateTimeOffset.UtcNow.AddHours(-3);
            interview.AnswerTextQuestion(interviewerId, commentedQuestionId, RosterVector.Empty, t0.AddHours(3), "before");
            interview.Complete(interviewerId, "", t0.AddMinutes(1), null);
            interview.Reject(supervisorId, "reject", t0.AddMinutes(2));
            var questionnaire = Create.Entity.PlainQuestionnaire(document);

            var before = (InterviewTextQuestion)service.GetEntityDetails(commentedQuestionId.FormatGuid(), interview, questionnaire, true);
            Assert.That(before.IsChangedSinceReject, Is.False);

            interview.AnswerTextQuestion(interviewerId, commentedQuestionId, RosterVector.Empty, t0, "after");

            var after = (InterviewTextQuestion)service.GetEntityDetails(commentedQuestionId.FormatGuid(), interview, questionnaire, true);
            Assert.That(after.IsChangedSinceReject, Is.True);

            interview.RemoveAnswer(commentedQuestionId, RosterVector.Empty, interviewerId, t0.AddMinutes(3));

            var unanswered = (InterviewTextQuestion)service.GetEntityDetails(commentedQuestionId.FormatGuid(), interview, questionnaire, true);
            Assert.That(unanswered.IsChangedSinceReject, Is.False);
        }

        [Test]
        public void should_mark_answers_after_HQ_rejection_of_supervisor_approved_interview()
        {
            var t0 = DateTimeOffset.UtcNow.AddHours(-3);
            interview.AnswerTextQuestion(interviewerId, commentedQuestionId, RosterVector.Empty, t0.AddHours(3), "before");
            interview.Complete(interviewerId, "", t0, null);
            interview.Approve(supervisorId, "approved", t0.AddMinutes(1));
            interview.HqReject(headquartersId, "reject", t0.AddMinutes(2));
            var questionnaire = Create.Entity.PlainQuestionnaire(document);

            var before = (InterviewTextQuestion)service.GetEntityDetails(commentedQuestionId.FormatGuid(), interview, questionnaire, true);
            Assert.That(before.IsChangedSinceReject, Is.False);

            var history = interview.GetUnCommittedChanges().Select(e => new CommittedEvent(
                Guid.Empty, null, e.EventIdentifier, e.EventSourceId, e.EventSequence, e.EventTimeStamp, null, e.Payload)).ToList();
            var answerEvent = Create.Event.TextQuestionAnswered(
                questionId: commentedQuestionId,
                rosterVector: RosterVector.Empty,
                answer: "after",
                userId: interviewerId,
                originDate: t0);
            var lastEvent = history.Last();
            history.Add(new CommittedEvent(
                Guid.Empty,
                null,
                Guid.NewGuid(),
                interview.Id,
                lastEvent.EventSequence + 1,
                DateTime.UtcNow,
                null,
                answerEvent));
            var replayedInterview = Create.AggregateRoot.StatefulInterview(questionnaire: document, shouldBeInitialized: false);
            replayedInterview.InitializeFromHistory(interview.Id, history);

            var after = (InterviewTextQuestion)service.GetEntityDetails(commentedQuestionId.FormatGuid(), replayedInterview, questionnaire, true);
            Assert.That(after.IsChangedSinceReject, Is.True);
        }

        [Test]
        public void should_restore_changed_since_rejection_state_from_event_history()
        {
            var t0 = DateTimeOffset.UtcNow.AddHours(-3);
            interview.AnswerTextQuestion(interviewerId, commentedQuestionId, RosterVector.Empty, t0.AddHours(3), "before");
            interview.Complete(interviewerId, "", t0.AddMinutes(1), null);
            interview.Reject(supervisorId, "reject", t0.AddMinutes(2));
            interview.AnswerTextQuestion(interviewerId, commentedQuestionId, RosterVector.Empty, t0, "after");

            var history = interview.GetUnCommittedChanges().Select(e => new CommittedEvent(
                Guid.Empty, null, e.EventIdentifier, e.EventSourceId, e.EventSequence, e.EventTimeStamp, null, e.Payload)).ToArray();
            var replayedInterview = Create.AggregateRoot.StatefulInterview(questionnaire: document, shouldBeInitialized: false);
            replayedInterview.InitializeFromHistory(interview.Id, history);

            var entity = (InterviewTextQuestion)service.GetEntityDetails(
                commentedQuestionId.FormatGuid(),
                replayedInterview,
                Create.Entity.PlainQuestionnaire(document),
                true);
            Assert.That(entity.IsChangedSinceReject, Is.True);
        }

        [Test]
        public void should_preserve_changed_answers_when_replaying_incremental_history()
        {
            var t0 = DateTimeOffset.UtcNow.AddHours(-3);
            interview.AnswerTextQuestion(interviewerId, commentedQuestionId, RosterVector.Empty, t0.AddHours(3), "before");
            interview.Complete(interviewerId, "", t0.AddMinutes(1), null);
            interview.Reject(supervisorId, "reject", t0.AddMinutes(2));
            interview.AnswerTextQuestion(interviewerId, commentedQuestionId, RosterVector.Empty, t0, "after");

            var history = interview.GetUnCommittedChanges().Select(e => new CommittedEvent(
                Guid.Empty, null, e.EventIdentifier, e.EventSourceId, e.EventSequence, e.EventTimeStamp, null, e.Payload)).ToArray();
            var replayedInterview = Create.AggregateRoot.StatefulInterview(questionnaire: document, shouldBeInitialized: false);
            replayedInterview.InitializeFromHistory(interview.Id, history);

            interview.CommentAnswer(interviewerId, commentedQuestionId, RosterVector.Empty, t0.AddMinutes(3), "comment");
            var nextEvent = interview.GetUnCommittedChanges().Last();
            replayedInterview.InitializeFromHistory(interview.Id, new[]
            {
                new CommittedEvent(
                    Guid.Empty,
                    null,
                    nextEvent.EventIdentifier,
                    nextEvent.EventSourceId,
                    nextEvent.EventSequence,
                    nextEvent.EventTimeStamp,
                    null,
                    nextEvent.Payload)
            });

            Assert.That(replayedInterview.IsQuestionChangedSinceLastRejection(
                Identity.Create(commentedQuestionId, RosterVector.Empty)), Is.True);
        }

        [Test]
        public void should_reset_changed_answers_after_a_later_rejection_in_live_state_and_replay()
        {
            var t0 = DateTimeOffset.UtcNow.AddHours(-3);
            interview.AnswerTextQuestion(interviewerId, commentedQuestionId, RosterVector.Empty, t0, "before");
            interview.Complete(interviewerId, "", t0.AddMinutes(1), null);
            interview.Reject(supervisorId, "first reject", t0.AddMinutes(2));
            interview.AnswerTextQuestion(interviewerId, commentedQuestionId, RosterVector.Empty, t0, "after first reject");
            var eventCountAfterFirstAnswer = interview.GetUnCommittedChanges().Count();
            var questionIdentity = Identity.Create(commentedQuestionId, RosterVector.Empty);

            Assert.That(interview.IsQuestionChangedSinceLastRejection(questionIdentity), Is.True);

            interview.Complete(interviewerId, "", t0.AddMinutes(4), null);
            interview.Reject(supervisorId, "second reject", t0.AddMinutes(5));
            var eventCountAfterSecondReject = interview.GetUnCommittedChanges().Count();

            Assert.That(interview.IsQuestionChangedSinceLastRejection(questionIdentity), Is.False);

            interview.AnswerTextQuestion(interviewerId, commentedQuestionId, RosterVector.Empty, t0, "after second reject");

            Assert.That(interview.IsQuestionChangedSinceLastRejection(questionIdentity), Is.True);

            var history = interview.GetUnCommittedChanges().Select(e => new CommittedEvent(
                Guid.Empty, null, e.EventIdentifier, e.EventSourceId, e.EventSequence, e.EventTimeStamp, null, e.Payload)).ToArray();
            var replayedInterview = Create.AggregateRoot.StatefulInterview(questionnaire: document, shouldBeInitialized: false);
            replayedInterview.InitializeFromHistory(interview.Id, history.Take(eventCountAfterFirstAnswer));

            Assert.That(replayedInterview.IsQuestionChangedSinceLastRejection(questionIdentity), Is.True);

            replayedInterview.InitializeFromHistory(
                interview.Id,
                history.Skip(eventCountAfterFirstAnswer).Take(eventCountAfterSecondReject - eventCountAfterFirstAnswer));

            Assert.That(replayedInterview.IsQuestionChangedSinceLastRejection(questionIdentity), Is.False);

            replayedInterview.InitializeFromHistory(interview.Id, history.Skip(eventCountAfterSecondReject));

            Assert.That(replayedInterview.IsQuestionChangedSinceLastRejection(questionIdentity), Is.True);
        }

        protected QuestionnaireDocument GetDocument()
        {
            return Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(commentedQuestionId)
            );
        }
    }
}
