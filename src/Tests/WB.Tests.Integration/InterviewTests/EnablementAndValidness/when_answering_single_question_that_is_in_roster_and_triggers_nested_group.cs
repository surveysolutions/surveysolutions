using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_single_question_that_is_in_roster_and_triggers_nested_group : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                var rosterSwitcherQuestionId = Guid.Parse("11111111111111111111111111111111");
                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var groupTriggerQuestionId = Guid.Parse("22222222222222222222222222222222");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var nestedGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.SingleQuestion(rosterSwitcherQuestionId, variable: "hwrkyn", options: new List<Answer>
                    {
                        Create.Option(value: "1", text: "Yes"),
                        Create.Option(value: "2", text: "No")
                    }),
                    Create.ListQuestion(rosterSizeQuestionId, variable: "jobs", enablementCondition: "hwrkyn == 1"),
                    Create.Roster(rosterId, variable: "about_jobs", enablementCondition: "hwrkyn == 1", rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeQuestionId,
                        children: new IComposite[]
                        {
                            Create.SingleQuestion(groupTriggerQuestionId, variable: "has_wage", options: new List<Answer>
                            {
                                Create.Option(value: "1", text: "Yes"),
                                Create.Option(value: "2", text: "No")
                            }),
                            Create.Group(nestedGroupId, enablementCondition: "has_wage == 1")
                        })
                    );

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerSingleOptionQuestion(userId, rosterSwitcherQuestionId, new decimal[0], DateTime.Now, 1);
                interview.AnswerTextListQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, new[] { new Tuple<decimal, string>(1, "The World Bank") });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(userId, groupTriggerQuestionId, new decimal[] { 1 }, DateTime.Now, 1);

                    return new InvokeResults
                    {
                        WasNestedGroupEnabled = eventContext.AnyEvent<GroupsEnabled>(x => x.Groups.Any(g => g.Id == nestedGroupId)),
                        WasNestedGroupDisabled = eventContext.AnyEvent<GroupsDisabled>(x => x.Groups.Any(g => g.Id == nestedGroupId)),
                    };
                }
            });

        It should_raise_GroupsEnabled_event_for_nested_roster_groupd = () =>
            results.WasNestedGroupEnabled.ShouldBeTrue();

        It should_not_raise_GroupsDisabled_event_for_nested_roster_groupd = () =>
            results.WasNestedGroupDisabled.ShouldBeFalse();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasNestedGroupEnabled { get; set; }
            public bool WasNestedGroupDisabled { get; set; }
        }
    }
}