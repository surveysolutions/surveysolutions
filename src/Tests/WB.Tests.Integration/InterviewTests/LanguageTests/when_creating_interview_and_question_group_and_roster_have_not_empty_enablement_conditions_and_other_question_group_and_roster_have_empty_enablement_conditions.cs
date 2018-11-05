using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_creating_interview_and_question_group_and_roster_have_not_empty_enablement_conditions_and_other_question_group_and_roster_have_empty_enablement_conditions : InterviewTestsContext
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
                    Abc.Create.Entity.Group(null, "Chapter X", null, null, false, new IComposite[]
                    {
                        Abc.Create.Entity.NumericIntegerQuestion(variable: "a"),
                        Abc.Create.Entity.Question(questionId: questionId, variable: "b", enablementCondition: "a > 0"),
                        Abc.Create.Entity.Group(),
                        Abc.Create.Entity.Group(groupId, enablementCondition: "a < 0"),
                        Abc.Create.Entity.Roster(variable: "x", fixedTitles: new[] { "1", "2" }),
                        Abc.Create.Entity.Roster(rosterId: rosterId, variable: "fixedConditional", fixedTitles: new[] { "1", "2" }, enablementCondition: "a == 0"),
                    }),
                });

                using (var eventContext = new EventContext())
                {
                    SetupInterview(questionnaireDocument);

                    return new InvokeResult
                    {
                        QuestionsEnabledEventCount = eventContext.Count<QuestionsEnabled>(),
                        GroupsEnabledEventCount = eventContext.Count<GroupsEnabled>(),
                        QuestionsDisabledEventCount = eventContext.Count<QuestionsDisabled>(),
                        GroupsDisabledEventCount = eventContext.Count<GroupsDisabled>(),
                        QuestionsDisabledEventQuestionIds = eventContext.GetSingleEvent<QuestionsDisabled>().Questions.Select(identity => identity.Id).ToArray(),
                        GroupsDisabledEventGroupIds = eventContext.GetSingleEvent<GroupsDisabled>().Groups.Select(identity => identity.Id).ToArray(),
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

        [NUnit.Framework.Test] public void should_raise_QuestionsDisabled_event () =>
            result.QuestionsDisabledEventCount.Should().Be(1);

        [NUnit.Framework.Test] public void should_raise_GroupsDisabled_event () =>
            result.GroupsDisabledEventCount.Should().Be(1);

        [NUnit.Framework.Test] public void should_put_only_id_of_question_with_enablement_condition_to_QuestionsDisabled_event () =>
            result.QuestionsDisabledEventQuestionIds.Should().BeEquivalentTo(questionId);

        [NUnit.Framework.Test] public void should_put_only_id_of_group_instances_with_enablement_conditions_to_QuestionsDisabled_event () =>
            result.GroupsDisabledEventGroupIds.Should().BeEquivalentTo(groupId, rosterId, rosterId);

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResult result;
        private static readonly Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid groupId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid rosterId = Guid.Parse("33333333333333333333333333333333");

        [Serializable]
        private class InvokeResult
        {
            public int QuestionsEnabledEventCount { get; set; }
            public int GroupsEnabledEventCount { get; set; }
            public int QuestionsDisabledEventCount { get; set; }
            public int GroupsDisabledEventCount { get; set; }
            public Guid[] QuestionsDisabledEventQuestionIds { get; set; }
            public Guid[] GroupsDisabledEventGroupIds { get; set; }
        }
    }
}
