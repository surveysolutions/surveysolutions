using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_list_question_that_is_roster_size_and_roster_has_condition_should_turn_it_off : InterviewTestsContext
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
                    Abc.Create.Entity.SingleQuestion(rosterSwitcherQuestionId, variable: "hwrkyn", options: new List<Answer>
                    {
                        Abc.Create.Entity.Option(value: "1", text: "Yes"),
                        Abc.Create.Entity.Option(value: "2", text: "No")
                    }),
                    Abc.Create.Entity.TextListQuestion(questionId: rosterSizeQuestionId, variable: "jobs",
                        enablementCondition: "hwrkyn == 1", validationExpression: null),
                    Abc.Create.Entity.Roster(rosterId, variable: "about_jobs", enablementCondition: "hwrkyn == 2", rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeQuestionId));

                var interview = SetupInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument);

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

        [NUnit.Framework.Test] public void should_raise_GroupsEnabled_event_for_nested_roster_groupd () =>
            results.WasRosterEnabled.Should().BeFalse();

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
