using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answer_on_integer_question_increases_roster_size_of_disabled_roster_with_disabled_nested_roster_with_disabled_questions_inside_triggered_by_the_same_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId, variable: "a"),
                    Abc.Create.Entity.NumericRoster(rosterId,enablementCondition: "a > 2", rosterSizeQuestionId: rosterSizeQuestionId, variable: "r1", children: new IComposite[]
                    {
                        Abc.Create.Entity.NumericIntegerQuestion(id: rosterQuestionId, enablementCondition: "a > 2", variable: null),
                        Abc.Create.Entity.NumericRoster(nestedRosterId, enablementCondition: "a > 2", rosterSizeQuestionId: rosterSizeQuestionId, variable: "r2", children: new[]
                        {
                            Abc.Create.Entity.NumericIntegerQuestion(id: nestedRosterQuestionId, enablementCondition: "a > 2", variable: null)
                        })
                    })
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, 1);

                    return new InvokeResults
                    {
                        NestedRosterInstanceRemoved = HasEvent<RosterInstancesRemoved>(eventContext.Events, where => where.Instances.Any(x => x.GroupId == nestedRosterId)),
                        RosterAndNestedRosterInstancesAdded =
                            HasEvent<RosterInstancesAdded>(eventContext.Events, _ => 
                                _.Instances.Count( x =>
                                        x.GroupId == rosterId && x.RosterInstanceId == 0 &&
                                        x.OuterRosterVector.Length == 0) == 1
                                &&
                                _.Instances.Count(x =>
                                        x.GroupId == nestedRosterId && x.RosterInstanceId == 0 &&
                                        x.OuterRosterVector.SequenceEqual(new decimal[] { 0 })) == 1),
                        FirstRowOfNestedRosterDisabled =
                            HasEvent<GroupsDisabled>(eventContext.Events, _ => _
                                .Groups.Count(x =>
                                            x.Id == nestedRosterId &&
                                            x.RosterVector.Identical(new decimal[] { 0, 0 })) == 1),
                        FirstRowOfRosterDisabled =
                            HasEvent<GroupsDisabled>(eventContext.Events, _ => _
                                .Groups.Count(x =>
                                            x.Id == rosterId &&
                                            x.RosterVector.Identical(new decimal[] { 0 })) == 1),
                        FirstQuestionFromFirstRowOfNestedRosterDisabled =
                            HasEvent<QuestionsDisabled>(eventContext.Events, _ => _
                                .Questions.Count(x =>
                                            x.Id == nestedRosterQuestionId &&
                                            x.RosterVector.Identical(new decimal[] { 0, 0 })) == 1),
                        FirstQuestionFromFirstRowOfRosterDisabled =
                            HasEvent<QuestionsDisabled>(eventContext.Events, _ => _
                                .Questions.Count(x =>
                                            x.Id == rosterQuestionId &&
                                            x.RosterVector.Identical(new decimal[] { 0 })) == 1)
                    };
                }
            });

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_event_for_roster_and_nested_roster () =>
            results.RosterAndNestedRosterInstancesAdded.Should().BeTrue();

        [NUnit.Framework.Test] public void should_raise_GroupsDisabled_event_for_first_row_of_nested_roster () =>
            results.FirstRowOfNestedRosterDisabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_raise_GroupsDisabled_event_for_first_row_of_roster () =>
            results.FirstRowOfRosterDisabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_raise_QuestionsDisabled_event_for_first_question_from_first_row_of_roster () =>
            results.FirstQuestionFromFirstRowOfNestedRosterDisabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_raise_QuestionsDisabled_event_for_first_question_from_first_row_of_nested_roster () =>
            results.FirstQuestionFromFirstRowOfRosterDisabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_not_raise_RosterInstancesRemoved_event_for_nested_roster () =>
            results.NestedRosterInstanceRemoved.Should().BeFalse();

        private static InvokeResults results;
        private static Guid userId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionnaireId = Guid.Parse("77778888000000000000000000000000");
        private static Guid rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid nestedRosterQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static Guid rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid nestedRosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

        [Serializable]
        internal class InvokeResults
        {
            public bool RosterAndNestedRosterInstancesAdded { get; set; }
            public bool NestedRosterInstanceRemoved { get; set; }
            public bool FirstRowOfNestedRosterDisabled { get; set; }
            public bool FirstRowOfRosterDisabled { get; set; }
            public bool FirstQuestionFromFirstRowOfRosterDisabled { get; set; }
            public bool FirstQuestionFromFirstRowOfNestedRosterDisabled { get; set; }
        }
    }
}
