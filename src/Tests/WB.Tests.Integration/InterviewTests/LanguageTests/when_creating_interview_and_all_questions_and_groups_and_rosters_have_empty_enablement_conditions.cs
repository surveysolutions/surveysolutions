using System;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_creating_interview_and_all_questions_and_groups_and_rosters_have_empty_enablement_conditions : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
                {
                    Abc.Create.Entity.Group(children: new IComposite[]
                    {
                        Abc.Create.Entity.Question(variable: "a", enablementCondition: null),
                        Abc.Create.Entity.Question(variable: "b", enablementCondition: null),
                        Abc.Create.Entity.Group(null, "Group X", "i", null, false, null),
                        Abc.Create.Entity.Group(null, "Group X", "j", null, false, null),
                        Abc.Create.Entity.Roster(variable: "x", enablementCondition: null),
                        Abc.Create.Entity.Roster(variable: "y", enablementCondition: null),
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

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [NUnit.Framework.Test] public void should_not_raise_QuestionsEnabled_events () =>
            result.QuestionsEnabledEventCount.Should().Be(0);

        [NUnit.Framework.Test] public void should_not_raise_GroupsEnabled_events () =>
            result.GroupsEnabledEventCount.Should().Be(0);

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
