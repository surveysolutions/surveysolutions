using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_multipleoptionquestion_triggering_roster_having_condition_with_rowindex : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                var rosterSwitcherQuestionId = Guid.Parse("11111111111111111111111111111111");
                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.MultyOptionsQuestion(rosterSwitcherQuestionId, variable: "hwrkyn", 
                    options: new List<Answer>
                    {
                        Abc.Create.Entity.Option(value: "1", text: "Yes"),
                        Abc.Create.Entity.Option(value: "2", text: "No"),
                        Abc.Create.Entity.Option(value: "3", text: "Maybe")
                    }),
                    
                    Abc.Create.Entity.Roster(rosterId, variable: "about_jobs", 
                        enablementCondition: "@rowindex != 0", rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSwitcherQuestionId));

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument);

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

        [NUnit.Framework.Test] public void should_raise_GroupsEnabled_event_for_nested_roster_groupd () =>
            results.WasRosterEnabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_not_raise_GroupsDisabled_event_for_nested_roster_groupd () =>
            results.WasRosterDisabled.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasRosterEnabled { get; set; }
            public bool WasRosterDisabled { get; set; }
        }
    }
}
