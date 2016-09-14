using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_creating_interview_and_question_group_and_roster_have_not_empty_enablement_conditions_and_other_question_group_and_roster_have_empty_enablement_conditions : InterviewTestsContext
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
                        Create.NumericIntegerQuestion(variable: "a", enablementCondition: null),
                        Create.Question(id: questionId, variable: "b", enablementCondition: "a > 0"),
                        Create.Group(variable: "i", enablementCondition: null),
                        Create.Group(id: groupId, variable: "groupConditional", enablementCondition: "a < 0"),
                        Create.Roster(variable: "x", fixedTitles: new[] { "1", "2" }, enablementCondition: null),
                        Create.Roster(id: rosterId, variable: "fixedConditional", fixedTitles: new[] { "1", "2" }, enablementCondition: "a == 0"),
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

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_not_raise_QuestionsEnabled_events = () =>
            result.QuestionsEnabledEventCount.ShouldEqual(0);

        It should_not_raise_GroupsEnabled_events = () =>
            result.GroupsEnabledEventCount.ShouldEqual(0);

        It should_raise_QuestionsDisabled_event = () =>
            result.QuestionsDisabledEventCount.ShouldEqual(1);

        It should_raise_GroupsDisabled_event = () =>
            result.GroupsDisabledEventCount.ShouldEqual(1);

        It should_put_only_id_of_question_with_enablement_condition_to_QuestionsDisabled_event = () =>
            result.QuestionsDisabledEventQuestionIds.ShouldContainOnly(questionId);

        It should_put_only_id_of_group_instances_with_enablement_conditions_to_QuestionsDisabled_event = () =>
            result.GroupsDisabledEventGroupIds.ShouldContainOnly(groupId, rosterId, rosterId);

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