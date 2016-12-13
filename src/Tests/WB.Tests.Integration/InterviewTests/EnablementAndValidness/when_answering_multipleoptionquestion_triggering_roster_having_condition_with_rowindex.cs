using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_multipleoptionquestion_triggering_roster_having_condition_with_rowindex : InterviewTestsContext
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
                    Create.MultyOptionsQuestion(rosterSwitcherQuestionId, variable: "hwrkyn", 
                    options: new List<Answer>
                    {
                        Create.Option(value: "1", text: "Yes"),
                        Create.Option(value: "2", text: "No"),
                        Create.Option(value: "3", text: "Maybe")
                    }),
                    
                    Create.Roster(rosterId, variable: "about_jobs", 
                        enablementCondition: "@rowindex != 0", rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSwitcherQuestionId));

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerMultipleOptionsQuestion(userId, rosterSwitcherQuestionId, new decimal[0], DateTime.Now, new int[] {3});

                using (var eventContext = new EventContext())
                {
                    interview.AnswerMultipleOptionsQuestion(userId, rosterSwitcherQuestionId, new decimal[0], DateTime.Now, new int[] { 3 , 1});

                    return new InvokeResults
                    {
                        WasRosterEnabled = eventContext.AnyEvent<GroupsEnabled>(x => x.Groups.Any(g => g.Id == rosterId && g.RosterVector.Coordinates.First() == 3)),
                        WasRosterDisabled = eventContext.AnyEvent<GroupsDisabled>(x => x.Groups.Any(g => g.Id == rosterId && g.RosterVector.Coordinates.First() == 1)),
                    };
                }
            });

        It should_raise_GroupsEnabled_event_for_nested_roster_groupd = () =>
            results.WasRosterEnabled.ShouldBeTrue();

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