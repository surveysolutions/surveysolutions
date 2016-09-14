using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_on_a_question_with_answer_that_makes_dependent_question_invalid : in_standalone_app_domain
    {
        Because of = () => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            Setup.MockedServiceLocator();

            var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var dependentQuestionId = Guid.Parse("22222222222222222222222222222222");

            var interview = SetupInterview(
                questionnaireDocument: Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(answeredQuestionId, "q1"),
                    Create.NumericIntegerQuestion(dependentQuestionId, "q2",
                        validationConditions: Create.ValidationCondition(expression: "q1 != q2").ToEnumerable()),
                }),
                events: new object[]
                {
                    Create.Event.NumericIntegerQuestionAnswered(questionId: dependentQuestionId, answer: 1),
                });

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestion(questionId: answeredQuestionId, answer: 1));

                return new InvokeResults
                {
                    WasAnswersDeclaredInvalidEventPublishedForDependentQuestion =
                        eventContext
                            .GetSingleEventOrNull<AnswersDeclaredInvalid>()?
                            .FailedValidationConditions
                            .ContainsKey(Create.Identity(dependentQuestionId))
                        ?? false,
                };
            }
        });

        It should_mark_dependent_question_as_invalid = () =>
            results.WasAnswersDeclaredInvalidEventPublishedForDependentQuestion.ShouldBeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasAnswersDeclaredInvalidEventPublishedForDependentQuestion { get; set; }
        }
    }
}