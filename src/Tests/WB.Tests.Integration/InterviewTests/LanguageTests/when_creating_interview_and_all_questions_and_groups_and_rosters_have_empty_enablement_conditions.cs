using System;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_creating_interview_and_all_questions_and_groups_and_rosters_have_empty_enablement_conditions : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Create.Chapter(children: new IComposite[]
                    {
                        Create.Question(variable: "a", enablementCondition: null),
                        Create.Question(variable: "b", enablementCondition: null),
                        Create.Group(variable: "i", enablementCondition: null),
                        Create.Group(variable: "j", enablementCondition: null),
                        Create.Roster(variable: "x", enablementCondition: null),
                        Create.Roster(variable: "y", enablementCondition: null),
                    }),
                });

                using (var eventContext = new EventContext())
                {
                    SetupInterview(questionnaireDocument);

                    return new InvokeResult
                    {
                        GroupsEnabledEventCount = eventContext.Count<GroupsEnabled>(),
                        QuestionsEnabledEventCount = eventContext.Count<QuestionsEnabled>(),
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

        It should_not_raise_GroupsEnabled_events = () =>
            result.GroupsEnabledEventCount.ShouldEqual(0);

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public int QuestionsEnabledEventCount { get; set; }
            public int GroupsEnabledEventCount { get; set; }
        }
    }
}