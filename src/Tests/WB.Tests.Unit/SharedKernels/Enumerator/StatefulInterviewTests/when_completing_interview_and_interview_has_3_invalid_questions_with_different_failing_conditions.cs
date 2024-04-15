using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_completing_interview_and_interview_has_3_invalid_questions_with_different_failing_conditions
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: invalidQuestion1Identity.Id, variable: "q1"),
                Create.Entity.TextQuestion(questionId: invalidQuestion2Identity.Id, variable: "q2"),
                Create.Entity.TextQuestion(questionId: invalidQuestion3Identity.Id, variable: "q3"),
            });

            interview = SetUp.StatefulInterview(questionnaireDocument: questionnaire);

            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));

            interview.Apply(Create.Event.AnswersDeclaredInvalid(failedConditions: new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
            {
                {
                    invalidQuestion1Identity, new List<FailedValidationCondition>
                    {
                        Create.Entity.FailedValidationCondition(failedConditionIndex: 1),
                    }
                },
                {
                    invalidQuestion2Identity, new List<FailedValidationCondition>
                    {
                        Create.Entity.FailedValidationCondition(failedConditionIndex: 2),
                    }
                },
                {
                    invalidQuestion3Identity, new List<FailedValidationCondition>
                    {
                        Create.Entity.FailedValidationCondition(failedConditionIndex: 3),
                    }
                },
            }));

            eventContext = new EventContext();
            BecauseOf();
        }

        private void BecauseOf() =>
            interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow, null);

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredInvalid_event () =>
            eventContext.ShouldContainEvent<AnswersDeclaredInvalid>();

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredInvalid_event_with_identities_of_invalid_questions () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>().Questions.Should().BeEquivalentTo(new[]
            {
                invalidQuestion1Identity,
                invalidQuestion2Identity,
                invalidQuestion3Identity,
            });

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredInvalid_event_with_first_condition_failing_for_first_question () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>()
                .FailedValidationConditions[invalidQuestion1Identity].Should().BeEquivalentTo(new[]
                {
                    Create.Entity.FailedValidationCondition(failedConditionIndex: 1),
                });

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredInvalid_event_with_second_condition_failing_for_second_question () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>()
                .FailedValidationConditions[invalidQuestion2Identity].Should().BeEquivalentTo(new[]
                {
                    Create.Entity.FailedValidationCondition(failedConditionIndex: 2),
                });

        [NUnit.Framework.Test] public void should_raise_AnswersDeclaredInvalid_event_with_third_condition_failing_for_third_question () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>()
                .FailedValidationConditions[invalidQuestion3Identity].Should().BeEquivalentTo(new[]
                {
                    Create.Entity.FailedValidationCondition(failedConditionIndex: 3),
                });

        [NUnit.Framework.Test] public void should_not_raise_AnswersDeclaredValid_event () =>
            eventContext.ShouldNotContainEvent<AnswersDeclaredValid>();

        [OneTimeTearDown]
        public void TearDown()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private static StatefulInterview interview;
        private static EventContext eventContext;
        private static Identity invalidQuestion1Identity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBB1111111111111111"), RosterVector.Empty);
        private static Identity invalidQuestion2Identity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBB2222222222222222"), RosterVector.Empty);
        private static Identity invalidQuestion3Identity = Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBB3333333333333333"), RosterVector.Empty);
    }
}
