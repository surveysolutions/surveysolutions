using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_creating_interview_and_question_has_conditions_referencin_random : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var id = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(id, children: new[]
                {
                    Create.Chapter(children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(id: questionId, variable: "a", enablementCondition: "Quest.IRnd() > 0.5"),
                        Create.NumericIntegerQuestion(variable: "b", enablementCondition: "Quest.IRnd() <= 0.5"),
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

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_not_raise_QuestionsEnabled_events = () =>
            result.QuestionsEnabledEventCount.ShouldEqual(0);
        
        It should_raise_QuestionsDisabled_event = () =>
            result.QuestionsDisabledEventCount.ShouldEqual(1);
        
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