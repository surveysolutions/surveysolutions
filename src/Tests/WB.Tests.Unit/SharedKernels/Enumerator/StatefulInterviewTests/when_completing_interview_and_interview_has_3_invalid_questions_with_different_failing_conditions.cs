using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [Ignore("temp")]
    internal class when_completing_interview_and_interview_has_3_invalid_questions_with_different_failing_conditions
    {
        Establish context = () =>
        {
            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextQuestion(questionId: invalidQuestion1Identity.Id),
                Create.TextQuestion(questionId: invalidQuestion2Identity.Id),
                Create.TextQuestion(questionId: invalidQuestion3Identity.Id),
            });

            interview = Setup.StatefulInterview(questionnaireDocument: questionnaire);

            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));

            interview.Apply(Create.Event.AnswersDeclaredInvalid(questions: new []
            {
                invalidQuestion1Identity,
                invalidQuestion2Identity,
                invalidQuestion3Identity,
            }));

            interview.Apply(Create.Event.AnswersDeclaredInvalid(failedConditions: new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
            {
                {
                    invalidQuestion1Identity, new List<FailedValidationCondition>
                    {
                        Create.FailedValidationCondition(failedConditionIndex: 1),
                    }
                },
                {
                    invalidQuestion2Identity, new List<FailedValidationCondition>
                    {
                        Create.FailedValidationCondition(failedConditionIndex: 2),
                    }
                },
                {
                    invalidQuestion3Identity, new List<FailedValidationCondition>
                    {
                        Create.FailedValidationCondition(failedConditionIndex: 3),
                    }
                },
            }));

            eventContext = Create.EventContext();
        };

        Because of = () =>
            interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow);

        It should_raise_AnswersDeclaredInvalid_event = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredInvalid>();

        It should_raise_AnswersDeclaredInvalid_event_with_identities_of_invalid_questions = () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>().Questions.ShouldContainOnly(new[]
            {
                invalidQuestion1Identity,
                invalidQuestion2Identity,
                invalidQuestion3Identity,
            });

        It should_raise_AnswersDeclaredInvalid_event_with_first_condition_failing_for_first_question = () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>()
                .FailedValidationConditions[invalidQuestion1Identity].ShouldContainOnly(new[]
                {
                    Create.FailedValidationCondition(failedConditionIndex: 1),
                });

        It should_raise_AnswersDeclaredInvalid_event_with_second_condition_failing_for_second_question = () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>()
                .FailedValidationConditions[invalidQuestion2Identity].ShouldContainOnly(new[]
                {
                    Create.FailedValidationCondition(failedConditionIndex: 2),
                });

        It should_raise_AnswersDeclaredInvalid_event_with_third_condition_failing_for_third_question = () =>
            eventContext.GetEvent<AnswersDeclaredInvalid>()
                .FailedValidationConditions[invalidQuestion3Identity].ShouldContainOnly(new[]
                {
                    Create.FailedValidationCondition(failedConditionIndex: 3),
                });

        It should_not_raise_AnswersDeclaredValid_event = () =>
            eventContext.ShouldNotContainEvent<AnswersDeclaredValid>();

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static StatefulInterview interview;
        private static EventContext eventContext;
        private static Identity invalidQuestion1Identity = Create.Identity(Guid.Parse("BBBBBBBBBBBBBBBB1111111111111111"), RosterVector.Empty);
        private static Identity invalidQuestion2Identity = Create.Identity(Guid.Parse("BBBBBBBBBBBBBBBB2222222222222222"), RosterVector.Empty);
        private static Identity invalidQuestion3Identity = Create.Identity(Guid.Parse("BBBBBBBBBBBBBBBB3333333333333333"), RosterVector.Empty);
    }
}