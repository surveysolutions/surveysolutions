using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_on_a_question_with_answer_that_enables_dependent_static_texts_in_and_out_of_roster : in_standalone_app_domain
    {
        Because of = () => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            Setup.MockedServiceLocator();

            var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var dependentStaticTextOutsideRosterId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            var dependentStaticTextInsideRosterId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

            var interview = SetupInterview(questionnaireDocument: Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.NumericIntegerQuestion(answeredQuestionId, "q1"),

                Create.StaticText(dependentStaticTextOutsideRosterId, enablementCondition: "q1 > 0"),

                Create.Roster(fixedRosterTitles: new [] { Create.FixedRosterTitle(1, "one") }, children: new IComposite[]
                {
                    Create.StaticText(dependentStaticTextInsideRosterId, enablementCondition: "q1 > 0"),
                })
            }));

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestion(questionId: answeredQuestionId, answer: 1));

                return new InvokeResults
                {
                    WasStaticTextsEnabledEventPublishedForDependentStaticTextOutsideRoster =
                        eventContext.GetSingleEventOrNull<StaticTextsEnabled>()?.StaticTexts.Any(staticText => staticText.Id == dependentStaticTextOutsideRosterId) ?? false,

                    WasStaticTextsEnabledEventPublishedForDependentStaticTextInsideRoster =
                        eventContext.GetSingleEventOrNull<StaticTextsEnabled>()?.StaticTexts.Any(staticText => staticText.Id == dependentStaticTextInsideRosterId) ?? false,
                };
            }
        });

        It should_enable_dependent_static_text_outside_roster = () =>
            results.WasStaticTextsEnabledEventPublishedForDependentStaticTextOutsideRoster.ShouldBeTrue();

        It should_enable_dependent_static_text_inside_roster = () =>
            results.WasStaticTextsEnabledEventPublishedForDependentStaticTextInsideRoster.ShouldBeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasStaticTextsEnabledEventPublishedForDependentStaticTextOutsideRoster { get; set; }
            public bool WasStaticTextsEnabledEventPublishedForDependentStaticTextInsideRoster { get; set; }
        }
    }
}