using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_on_a_question_with_answer_that_enables_dependent_static_texts_in_and_out_of_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        public void BecauseOf() => results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
        {
            SetUp.MockedServiceLocator();

            var answeredQuestionId = Guid.Parse("11111111111111111111111111111111");
            var dependentStaticTextOutsideRosterId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            var dependentStaticTextInsideRosterId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");

            var interview = SetupInterviewWithExpressionStorage(questionnaireDocument: Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Abc.Create.Entity.NumericIntegerQuestion(answeredQuestionId, "q1"),
                Abc.Create.Entity.StaticText(dependentStaticTextOutsideRosterId, enablementCondition: "q1 > 0"),
                Abc.Create.Entity.Roster(fixedRosterTitles: new [] { IntegrationCreate.FixedTitle(1, "one") }, children: new IComposite[]
                {
                     Abc.Create.Entity.StaticText(dependentStaticTextInsideRosterId, enablementCondition: "q1 > 0"),
                })
            }));

            using (var eventContext = new EventContext())
            {
                interview.AnswerNumericIntegerQuestion(Abc.Create.Command.AnswerNumericIntegerQuestionCommand(questionId: answeredQuestionId, answer: 1));

                return new InvokeResults
                {
                    WasStaticTextsEnabledEventPublishedForDependentStaticTextOutsideRoster =
                        eventContext.GetSingleEventOrNull<StaticTextsEnabled>()?.StaticTexts.Any(staticText => staticText.Id == dependentStaticTextOutsideRosterId) ?? false,

                    WasStaticTextsEnabledEventPublishedForDependentStaticTextInsideRoster =
                        eventContext.GetSingleEventOrNull<StaticTextsEnabled>()?.StaticTexts.Any(staticText => staticText.Id == dependentStaticTextInsideRosterId) ?? false,
                };
            }
        });

        [NUnit.Framework.Test] public void should_enable_dependent_static_text_outside_roster () =>
            results.WasStaticTextsEnabledEventPublishedForDependentStaticTextOutsideRoster.Should().BeTrue();

        [NUnit.Framework.Test] public void should_enable_dependent_static_text_inside_roster () =>
            results.WasStaticTextsEnabledEventPublishedForDependentStaticTextInsideRoster.Should().BeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasStaticTextsEnabledEventPublishedForDependentStaticTextOutsideRoster { get; set; }
            public bool WasStaticTextsEnabledEventPublishedForDependentStaticTextInsideRoster { get; set; }
        }
    }
}
