using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_on_a_question_with_answer_that_enables_dependent_question : in_standalone_app_domain
    {
        Because of = () => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            Setup.MockedServiceLocator();

            var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var dependentQuestionId = Guid.Parse("22222222222222222222222222222222");

            var interview = SetupInterview(questionnaireDocument: Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.NumericIntegerQuestion(answeredQuestionId, "q1"),
                Create.NumericIntegerQuestion(dependentQuestionId, enablementCondition: "q1 > 0"),
            }));

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestion(questionId: answeredQuestionId, answer: 1));

                return new InvokeResults
                {
                    WasQuestionsEnabledEventPublishedForDependentQuestion =
                        eventContext.GetSingleEventOrNull<QuestionsEnabled>()?.Questions.Any(question => question.Id == dependentQuestionId) ?? false,
                };
            }
        });

        It should_enable_dependent_question = () =>
            results.WasQuestionsEnabledEventPublishedForDependentQuestion.ShouldBeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasQuestionsEnabledEventPublishedForDependentQuestion { get; set; }
        }
    }
}