using System;
using System.Collections.Generic;
using FluentAssertions;
using Ncqrs.Domain;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [TestFixture]
    [TestOf(typeof(Interview))]
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
        }

        [TestCaseSource(nameof(EventsReferencingMissingEntities))]
        public void should_replay_without_event_apply_exception(IEvent persistedEvent)
        {
            var questionnaireId = Guid.NewGuid();
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter();
            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireDocument);
            var interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
            var history = Prepare.Events(
                Create.Event.InterviewCreated(questionnaireId: questionnaireId, questionnaireVersion: 1),
                persistedEvent).ForSource(interview.EventSourceId);

            Action act = () => interview.InitializeFromHistory(interview.EventSourceId, history);

            act.Should().NotThrow<OnEventApplyException>();
        }
    }
}
