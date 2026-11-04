using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Repositories;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestFixture]
    [TestOf(typeof(TesterStatefulInterview))]
    internal class when_replaying_events_for_missing_entities
    {
        private static IEnumerable<TestCaseData> EventsReferencingMissingEntities()
        {
            var originDate = DateTimeOffset.UtcNow;
            var missingQuestion = new Identity(Guid.NewGuid(), Array.Empty<decimal>());
            var missingGroup = new Identity(Guid.NewGuid(), Array.Empty<decimal>());
            var missingVariable = new Identity(Guid.NewGuid(), Array.Empty<decimal>());
            var missingStaticText = new Identity(Guid.NewGuid(), Array.Empty<decimal>());
            var failedValidationConditions = new[] { new FailedValidationCondition(0) };

            yield return new TestCaseData(new QuestionsDisabled(new[] { missingQuestion }, originDate));
            yield return new TestCaseData(new GroupsDisabled(new[] { missingGroup }, originDate));
            yield return new TestCaseData(new VariablesDisabled(new[] { missingVariable }, originDate));
            yield return new TestCaseData(new StaticTextsDisabled(new[] { missingStaticText }, originDate));
            yield return new TestCaseData(new AnswersDeclaredValid(new[] { missingQuestion }, originDate));
            yield return new TestCaseData(new AnswersDeclaredInvalid(
                new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
                {
                    [missingQuestion] = failedValidationConditions
                },
                originDate));
            yield return new TestCaseData(new AnswersDeclaredPlausible(new[] { missingQuestion }, originDate));
            yield return new TestCaseData(new AnswersDeclaredImplausible(
                new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>
                {
                    new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(missingQuestion, failedValidationConditions)
                },
                originDate));
            yield return new TestCaseData(new StaticTextsDeclaredValid(new[] { missingStaticText }, originDate));
            yield return new TestCaseData(new StaticTextsDeclaredInvalid(
                new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>
                {
                    new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(missingStaticText, failedValidationConditions)
                },
                originDate));
            yield return new TestCaseData(new StaticTextsDeclaredPlausible(new[] { missingStaticText }, originDate));
            yield return new TestCaseData(new StaticTextsDeclaredImplausible(
                new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>
                {
                    new KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>(missingStaticText, failedValidationConditions)
                },
                originDate));
            yield return new TestCaseData(new NumericIntegerQuestionAnswered(
                Guid.NewGuid(), missingQuestion.Id, Array.Empty<decimal>(), originDate, 42));
            yield return new TestCaseData(new AnswersRemoved(null, new[] { missingQuestion }, originDate));
            yield return new TestCaseData(new AnswerRemoved(
                Guid.NewGuid(), missingQuestion.Id, Array.Empty<decimal>(), originDate));
            yield return new TestCaseData(new AnswersMarkedAsProtected(new[] { missingQuestion }, originDate));
            yield return new TestCaseData(new AnswerCommented(
                Guid.NewGuid(), missingQuestion.Id, Array.Empty<decimal>(), originDate, "comment", Guid.NewGuid()));
            yield return new TestCaseData(new AnswerCommentResolved(
                Guid.NewGuid(), missingQuestion.Id, missingQuestion.RosterVector, originDate, Array.Empty<Guid>()));
            yield return new TestCaseData(new LinkedToListOptionsChanged(
                new[] { new ChangedLinkedToListOptions(missingQuestion, Array.Empty<decimal>()) }, originDate));
            yield return new TestCaseData(Create.Event.InterviewPaused(originDate));
            yield return new TestCaseData(Create.Event.InterviewResumed(originDate));
            yield return new TestCaseData(Create.Event.InterviewOpenedBySupervisor());
            yield return new TestCaseData(Create.Event.InterviewClosedBySupervisor());
        }

        [TestCaseSource(nameof(EventsReferencingMissingEntities))]
        public void should_replay_without_event_apply_exception(IEvent persistedEvent)
        {
            var interview = CreateInterview(persistedEvent);

            interview.InitialVersion.Should().Be(2);
        }

        [Test]
        public void should_complete_without_event_not_handled_exception()
        {
            var interview = CreateInterview(
                Create.Event.InterviewStatusChanged(InterviewStatus.InterviewerAssigned));

            Action act = () => interview.Complete(Guid.NewGuid(), string.Empty, DateTimeOffset.UtcNow, null);

            act.Should().NotThrow();
        }

        private static TesterStatefulInterview CreateInterview(params IEvent[] persistedEvents)
        {
            var questionnaireId = Guid.NewGuid();
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter();
            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireDocument);
            var serviceLocator = Mock.Of<IServiceLocator>(x =>
                x.GetInstance<IQuestionnaireStorage>() == questionnaireRepository);
            var interview = new TesterStatefulInterview(
                Create.Service.SubstitutionTextFactory(),
                Create.Service.InterviewTreeBuilder(),
                Create.Storage.QuestionnaireQuestionOptionsRepository())
            {
                ServiceLocatorInstance = serviceLocator
            };
            interview.SetId(Guid.NewGuid());
            var history = Prepare.Events(new[]
                {
                    Create.Event.InterviewCreated(questionnaireId: questionnaireId, questionnaireVersion: 1)
                }
                .Concat(persistedEvents)
                .ToArray()).ForSource(interview.EventSourceId);
            interview.InitializeFromHistory(interview.EventSourceId, history);
            return interview;
        }
    }
}
