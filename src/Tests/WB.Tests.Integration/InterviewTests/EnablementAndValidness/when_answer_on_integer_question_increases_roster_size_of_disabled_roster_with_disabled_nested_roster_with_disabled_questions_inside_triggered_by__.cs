using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answer_on_integer_question_increases_roster_size_of_disabled_roster_with_disabled_nested_roster_with_disabled_questions_inside_triggered_by_the_same_question : InterviewTestsContext
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

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var rosterQuestionId = Guid.Parse("22222222222222222222222222222222");
                var nestedRosterQuestionId = Guid.Parse("33333333333333333333333333333333");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var nestedRosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                const string enablementCondition = "a > 2";

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.NumericIntegerQuestion(rosterSizeQuestionId, variable: "a"),
                    Create.Roster(rosterId, rosterSizeSourceType: RosterSizeSourceType.Question, enablementCondition: enablementCondition,
                        rosterSizeQuestionId: rosterSizeQuestionId, children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(rosterQuestionId, enablementCondition: enablementCondition),
                            Create.Roster(nestedRosterId, rosterSizeSourceType: RosterSizeSourceType.Question, enablementCondition: enablementCondition,
                                rosterSizeQuestionId: rosterSizeQuestionId, children: new[]
                                {
                                    Create.NumericIntegerQuestion(nestedRosterQuestionId, enablementCondition: enablementCondition)
                                })
                        })
                    );

                var interview = SetupInterview(questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, 1);

                    return new InvokeResults()
                    {
                        NestedRosterInstanceRemoved =
                            HasEvent<RosterInstancesRemoved>(eventContext.Events,
                                where => where.Instances.Any(instance => instance.GroupId == nestedRosterId)),
                        RosterAndNestedRosterInstancesAdded =
                            HasEvent<RosterInstancesAdded>(eventContext.Events, where
                                =>
                                where.Instances.Count(
                                    instance =>
                                        instance.GroupId == rosterId && instance.RosterInstanceId == 0 &&
                                        instance.OuterRosterVector.Length == 0) == 1
                                &&
                                where.Instances.Count(
                                    instance =>
                                        instance.GroupId == nestedRosterId && instance.RosterInstanceId == 0 &&
                                        instance.OuterRosterVector.SequenceEqual(new decimal[] {0})) == 1),
                        FirstRowOfNestedRosterDisabled =
                            HasEvent<GroupsDisabled>(eventContext.Events,
                                where =>
                                    where.Groups.Count(
                                        instance =>
                                            instance.Id == nestedRosterId &&
                                            instance.RosterVector.SequenceEqual(new decimal[] {0, 0})) == 1),
                        FirstRowOfRosterDisabled =
                            HasEvent<GroupsDisabled>(eventContext.Events,
                                where =>
                                    where.Groups.Count(
                                        instance =>
                                            instance.Id == rosterId &&
                                            instance.RosterVector.SequenceEqual(new decimal[] {0})) == 1),
                        FirstQuestionFromFirstRowOfNestedRosterDisabled =
                            HasEvent<QuestionsDisabled>(eventContext.Events,
                                where =>
                                    where.Questions.Count(
                                        instance =>
                                            instance.Id == nestedRosterQuestionId &&
                                            instance.RosterVector.SequenceEqual(new decimal[] {0, 0})) == 1),
                        FirstQuestionFromFirstRowOfRosterDisabled =
                            HasEvent<QuestionsDisabled>(eventContext.Events,
                                where =>
                                    where.Questions.Count(
                                        instance =>
                                            instance.Id == rosterQuestionId &&
                                            instance.RosterVector.SequenceEqual(new decimal[] {0})) == 1)
                    };
                }
            });

        It should_raise_RosterInstancesAdded_event_for_roster_and_nested_roster = () =>
            results.RosterAndNestedRosterInstancesAdded.ShouldBeTrue();

        It should_raise_GroupsDisabled_event_for_first_row_of_nested_roster = () =>
            results.FirstRowOfNestedRosterDisabled.ShouldBeTrue();

        It should_raise_GroupsDisabled_event_for_first_row_of_roster = () =>
            results.FirstRowOfRosterDisabled.ShouldBeTrue();

        It should_raise_QuestionsDisabled_event_for_first_question_from_first_row_of_roster = () =>
            results.FirstQuestionFromFirstRowOfNestedRosterDisabled.ShouldBeTrue();

        It should_raise_QuestionsDisabled_event_for_first_question_from_first_row_of_nested_roster = () =>
            results.FirstQuestionFromFirstRowOfRosterDisabled.ShouldBeTrue();

        It should_not_raise_RosterInstancesRemoved_event_for_nested_roster = () =>
            results.NestedRosterInstanceRemoved.ShouldBeFalse();

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
            public bool RosterAndNestedRosterInstancesAdded { get; set; }
            public bool NestedRosterInstanceRemoved { get; set; }
            public bool FirstRowOfNestedRosterDisabled { get; set; }
            public bool FirstRowOfRosterDisabled { get; set; }
            public bool FirstQuestionFromFirstRowOfRosterDisabled { get; set; }
            public bool FirstQuestionFromFirstRowOfNestedRosterDisabled { get; set; }
        }
    }
}