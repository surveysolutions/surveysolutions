using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    [Ignore("KP-6970")]
    internal class when_answering_on_a_question_with_answer_that_makes_dependent_static_text_invalid : in_standalone_app_domain
    {
        Because of = () => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            Setup.MockedServiceLocator();

            var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var dependentStaticTextId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            var interview = SetupInterview(
                questionnaireDocument: Create.QuestionnaireDocument(children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(answeredQuestionId, "q1"),
                    Create.StaticText(dependentStaticTextId,
                        validationConditions: Create.ValidationCondition(expression: "q1 > 0").ToEnumerable()),
                }),
                events: new object[]
                {
                    Create.Event.NumericIntegerQuestionAnswered(questionId: dependentStaticTextId, answer: 1),
                });

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestion(questionId: answeredQuestionId, answer: 1));

                return new InvokeResults
                {
                    WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticText =
                        eventContext
                            .GetSingleEventOrNull<StaticTextsDeclaredInvalid>()?
                            .GetFailedValidationConditionsDictionary()
                            .ContainsKey(Create.Identity(dependentStaticTextId))
                        ?? false,
                };
            }
        });

        It should_mark_dependent_static_text_as_invalid = () =>
            results.WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticText.ShouldBeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasStaticTextsDeclaredInvalidEventPublishedForDependentStaticText { get; set; }
        }
    }
}