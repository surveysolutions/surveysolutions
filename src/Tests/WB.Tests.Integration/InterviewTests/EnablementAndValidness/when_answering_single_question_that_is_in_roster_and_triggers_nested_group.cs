using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_single_question_that_is_in_roster_and_triggers_nested_group : InterviewTestsContext
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
                var groupTriggerQuestionId = Guid.Parse("22222222222222222222222222222222");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var nestedGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.SingleQuestion(rosterSwitcherQuestionId, variable: "hwrkyn", options: new List<Answer>
                    {
                        Abc.Create.Entity.Option(value: "1", text: "Yes"),
                        Abc.Create.Entity.Option(value: "2", text: "No")
                    }),
                    Abc.Create.Entity.TextListQuestion(questionId: rosterSizeQuestionId, variable: "jobs",
                        enablementCondition: "hwrkyn == 1", validationExpression: null),
                    Abc.Create.Entity.Roster(rosterId, variable: "about_jobs", enablementCondition: "hwrkyn == 1", rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: rosterSizeQuestionId,
                        children: new IComposite[]
                        {
                            Abc.Create.Entity.SingleQuestion(groupTriggerQuestionId, variable: "has_wage", options: new List<Answer>
                            {
                                Abc.Create.Entity.Option(value: "1", text: "Yes"),
                                Abc.Create.Entity.Option(value: "2", text: "No")
                            }),
                            Abc.Create.Entity.Group(nestedGroupId, "Group X", null, "has_wage == 1", false, null)
                        })
                    );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument);

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

        [NUnit.Framework.Test] public void should_raise_GroupsEnabled_event_for_nested_roster_groupd () =>
            results.WasNestedGroupEnabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_not_raise_GroupsDisabled_event_for_nested_roster_groupd () =>
            results.WasNestedGroupDisabled.Should().BeFalse();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

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
