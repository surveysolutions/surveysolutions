using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_list_question_that_is_roster_size_and_roster_has_condition_should_turn_it_off : InterviewTestsContext
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
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.SingleQuestion(rosterSwitcherQuestionId, variable: "hwrkyn", options: new List<Answer>
                    {
                        Create.Option(value: "1", text: "Yes"),
                        Create.Option(value: "2", text: "No")
                    }),
                    Create.ListQuestion(rosterSizeQuestionId, variable: "jobs", enablementCondition: "hwrkyn == 1"),
                    Create.Roster(rosterId, variable: "about_jobs", enablementCondition: "hwrkyn == 2", rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeQuestionId));

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerSingleOptionQuestion(userId, rosterSwitcherQuestionId, new decimal[0], DateTime.Now, 1);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextListQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, new[] { new Tuple<decimal, string>(1, "The World Bank") });

                    return new InvokeResults
                    {
                        WasRosterEnabled = eventContext.AnyEvent<GroupsEnabled>(x => x.Groups.Any(g => g.Id == rosterId)),
                        WasRosterDisabled = eventContext.AnyEvent<GroupsDisabled>(x => x.Groups.Any(g => g.Id == rosterId)),
                    };
                }
            });

        It should_raise_GroupsEnabled_event_for_nested_roster_groupd = () =>
            results.WasRosterEnabled.ShouldBeFalse();

        It should_not_raise_GroupsDisabled_event_for_nested_roster_groupd = () =>
            results.WasRosterDisabled.ShouldBeTrue();

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
            public bool WasRosterEnabled { get; set; }
            public bool WasRosterDisabled { get; set; }
        }
    }
}