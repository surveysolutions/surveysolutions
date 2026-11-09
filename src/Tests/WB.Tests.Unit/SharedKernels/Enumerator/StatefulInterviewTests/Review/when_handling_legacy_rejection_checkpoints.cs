using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Ncqrs.Eventing;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.Review
{
    [TestOf(typeof(StatefulInterview))]
    internal class when_handling_legacy_rejection_checkpoints
    {
        [Test]
        public void when_replaying_legacy_answer_should_leave_change_detection_unavailable()
        {
            var interviewId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var questionId = Guid.NewGuid();
            var interview = CreateInterview(interviewId, questionnaireId, questionId);
            var rejectionIdentifier = Guid.NewGuid();
            var answer = CreateAnswer(questionId, null);
            var history = new List<CommittedEvent>
            {
                CreateHistoryEvent(interviewId, 1, Create.Event.InterviewCreated(questionnaireId, 1)),
                CreateHistoryEvent(interviewId, 2, new InterviewRejectedByHQ(Guid.NewGuid(), "", DateTimeOffset.UtcNow), rejectionIdentifier),
                CreateHistoryEvent(interviewId, 3, answer)
            };

            interview.InitializeFromHistory(interviewId, history);

            Assert.That(answer.LastRejectionEventIdentifier, Is.Null);
            Assert.That(interview.IsQuestionChangedSinceLastRejection(Create.Entity.Identity(questionId)), Is.False);
        }

        [Test]
        public void when_synchronizing_answers_should_preserve_all_checkpoint_states()
        {
            var interviewId = Guid.NewGuid();
            var questionnaireId = Guid.NewGuid();
            var questionIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var interview = CreateInterview(interviewId, questionnaireId, questionIds);
            var userId = Guid.NewGuid();
            var rejectionIdentifier = Guid.NewGuid();
            var currentCheckpoint = CreateAnswer(questionIds[0], rejectionIdentifier);
            var olderRejectionIdentifier = Guid.NewGuid();
            var olderCheckpoint = CreateAnswer(questionIds[1], olderRejectionIdentifier);
            var noRejectionCheckpoint = CreateAnswer(questionIds[2], Guid.Empty);
            var unavailableCheckpoint = CreateAnswer(questionIds[3], null);
            var synchronizedEvents = new[]
            {
                Create.Event.InterviewCreated(questionnaireId, 1)
                    .ToAggregateRootEvent(eventSourceId: interviewId),
                new InterviewRejectedByHQ(userId, "", DateTimeOffset.UtcNow)
                    .ToAggregateRootEvent(eventSourceId: interviewId, eventId: rejectionIdentifier),
                currentCheckpoint.ToAggregateRootEvent(eventSourceId: interviewId),
                olderCheckpoint.ToAggregateRootEvent(eventSourceId: interviewId),
                noRejectionCheckpoint.ToAggregateRootEvent(eventSourceId: interviewId),
                unavailableCheckpoint.ToAggregateRootEvent(eventSourceId: interviewId)
            };
            var command = new SynchronizeInterviewEventsCommand(
                interviewId,
                userId,
                questionnaireId,
                1,
                synchronizedEvents,
                InterviewStatus.Completed,
                createdOnClient: true,
                interviewKey: null,
                newSupervisorId: null);

            interview.SynchronizeInterviewEvents(command);

            Assert.That(currentCheckpoint.LastRejectionEventIdentifier, Is.EqualTo(rejectionIdentifier));
            Assert.That(olderCheckpoint.LastRejectionEventIdentifier, Is.EqualTo(olderRejectionIdentifier));
            Assert.That(noRejectionCheckpoint.LastRejectionEventIdentifier, Is.EqualTo(Guid.Empty));
            Assert.That(unavailableCheckpoint.LastRejectionEventIdentifier, Is.Null);
            Assert.That(interview.IsQuestionChangedSinceLastRejection(Create.Entity.Identity(questionIds[0])), Is.True);
            Assert.That(interview.IsQuestionChangedSinceLastRejection(Create.Entity.Identity(questionIds[1])), Is.False);
            Assert.That(interview.IsQuestionChangedSinceLastRejection(Create.Entity.Identity(questionIds[2])), Is.False);
            Assert.That(interview.IsQuestionChangedSinceLastRejection(Create.Entity.Identity(questionIds[3])), Is.False);
        }

        private static StatefulInterview CreateInterview(Guid interviewId, Guid questionnaireId, params Guid[] questionIds)
        {
            var questions = new List<IComposite>();
            foreach (var questionId in questionIds)
                questions.Add(Create.Entity.TextQuestion(questionId));

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: questions.ToArray());
            return Create.AggregateRoot.StatefulInterview(
                interviewId,
                questionnaireId: questionnaireId,
                questionnaire: questionnaire,
                shouldBeInitialized: false);
        }

        private static TextQuestionAnswered CreateAnswer(Guid questionId, Guid? rejectionIdentifier)
        {
            return new TextQuestionAnswered(
                Guid.NewGuid(),
                questionId,
                Array.Empty<decimal>(),
                DateTimeOffset.UtcNow,
                "answer")
            {
                LastRejectionEventIdentifier = rejectionIdentifier
            };
        }

        private static CommittedEvent CreateHistoryEvent(
            Guid interviewId,
            int sequence,
            WB.Core.Infrastructure.EventBus.IEvent payload,
            Guid? eventIdentifier = null)
        {
            return new CommittedEvent(
                commitId: Guid.NewGuid(),
                origin: "test",
                eventIdentifier: eventIdentifier ?? Guid.NewGuid(),
                eventSourceId: interviewId,
                eventSequence: sequence,
                eventTimeStamp: DateTime.UtcNow,
                payload: payload,
                globalSequence: sequence);
        }
    }
}
