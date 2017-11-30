using System;
using System.Collections.Generic;
using Machine.Specifications;
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
    internal class when_completing_interview_with_disabled_invalid_question_and_static_text : StatefulInterviewTestsContext
    {
        [Test]
        public void Should_raise_interview_declated_valid_event()
        {
            Identity invalidQuestionIdentity = Id.Identity1;
            Identity invalidStaticTextdentity = Id.Identity2;

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: invalidQuestionIdentity.Id),
                Create.Entity.StaticText(invalidStaticTextdentity.Id),
            });

            var interview = Setup.StatefulInterview(questionnaireDocument: questionnaire);

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

            interview.Apply(Create.Event.QuestionsDisabled(new[] { invalidQuestionIdentity }));
            interview.Apply(Create.Event.StaticTextsDisabled(new[] { invalidStaticTextdentity }));
            using (var eventContext = new EventContext())
            {

                // Act
                interview.Complete(Guid.NewGuid(), string.Empty, DateTime.UtcNow);

                // Assert
                eventContext.ShouldContainEvent<InterviewDeclaredValid>();
                eventContext.ShouldNotContainEvent<InterviewDeclaredInvalid>();
            }
        }
    }
}