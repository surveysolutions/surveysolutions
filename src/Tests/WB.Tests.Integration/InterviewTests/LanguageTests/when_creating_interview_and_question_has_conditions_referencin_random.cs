using System;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_creating_interview_and_question_has_conditions_referencin_random : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var id = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id, children: new[]
                {
                    Abc.Create.Entity.Group(null, "Chapter X", null, null, false, new IComposite[]
                    {
                        Abc.Create.Entity.NumericIntegerQuestion(id: questionId, variable: "a", enablementCondition: "Quest.IRnd() > 0.5"),
                        Abc.Create.Entity.NumericIntegerQuestion(variable: "b", enablementCondition: "Quest.IRnd() <= 0.5"),
                    }),
                });

                using (var eventContext = new EventContext())
                {
                    SetupInterview(questionnaireDocument);

                    return new InvokeResult
                    {
                        QuestionsEnabledEventCount = eventContext.Count<QuestionsEnabled>(),
                        QuestionsDisabledEventCount = eventContext.Count<QuestionsDisabled>()
                    };
                }
            });

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [NUnit.Framework.Test] public void should_not_raise_QuestionsEnabled_events () =>
            result.QuestionsEnabledEventCount.Should().Be(0);
        
        [NUnit.Framework.Test] public void should_raise_QuestionsDisabled_event () =>
            result.QuestionsDisabledEventCount.Should().Be(1);
        
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResult result;
        private static readonly Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid groupId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid rosterId = Guid.Parse("33333333333333333333333333333333");

        [Serializable]
        private class InvokeResult
        {
            public int QuestionsEnabledEventCount { get; set; }
            public int QuestionsDisabledEventCount { get; set; }
        }
    }
}
