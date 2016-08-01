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
    internal class when_completing_interview_with_disabled_invalid_question_and_static_text : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: invalidQuestionIdentity.Id),
                Create.Entity.StaticText(invalidStaticTextdentity.Id),
            });
            
            interview = Setup.StatefulInterview(questionnaireDocument: questionnaire);

            interview.Apply(Create.Event.InterviewStatusChanged(status: InterviewStatus.InterviewerAssigned));

            interview.Apply(
                Create.Event.AnswersDeclaredInvalid(failedConditions:
                    new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
                    {
                        {
                            invalidQuestionIdentity, new List<FailedValidationCondition>
                            {
                                Create.Entity.FailedValidationCondition(failedConditionIndex: 1),
                            }
                        }
                    }));

            interview.Apply(Create.Event.StaticTextsDeclaredInvalid(invalidStaticTextdentity));

            interview.Apply(Create.Event.QuestionsDisabled(new [] { invalidQuestionIdentity }));
            interview.Apply(Create.Event.StaticTextsDisabled(new [] { invalidStaticTextdentity }));
            eventContext = Create.Other.EventContext();
        };

        Because of = () =>
            interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow);

        It should_mark_interview_as_valid = () => eventContext.ShouldContainEvent<InterviewDeclaredValid>();
        It should_not_mark_interview_as_invalid = () => eventContext.ShouldNotContainEvent<InterviewDeclaredInvalid>();

        static StatefulInterview interview;
        static EventContext eventContext;

        static readonly Identity invalidQuestionIdentity =
            Create.Entity.Identity(Guid.Parse("BBBBBBBBBBBBBBBB1111111111111111"), RosterVector.Empty);

        static readonly Identity invalidStaticTextdentity =
           Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);

    }
}