using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_on_a_question_with_answer_that_enables_dependent_static_text : in_standalone_app_domain
    {
        Because of = () => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            Setup.MockedServiceLocator();

            var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var dependentStaticTextId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            var interview = SetupInterview(questionnaireDocument: Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.NumericIntegerQuestion(answeredQuestionId, "q1"),
                Create.StaticText(dependentStaticTextId, enablementCondition: "q1 > 0"),
            }));

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestion(questionId: answeredQuestionId, answer: 1));

                return new InvokeResults
                {
                    WasStaticTextsEnabledEventPublishedForDependentStaticText =
                        eventContext.GetSingleEventOrNull<StaticTextsEnabled>()?.StaticTexts.Any(staticText => staticText.Id == dependentStaticTextId) ?? false,
                };
            }
        });

        It should_enable_dependent_static_text = () =>
            results.WasStaticTextsEnabledEventPublishedForDependentStaticText.ShouldBeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasStaticTextsEnabledEventPublishedForDependentStaticText { get; set; }
        }
    }
}