using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_3_level_nested_rosters_are_triggered_by_same_yesno_question_and_answer_is_has_1_option : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var roster1Id = Guid.Parse("11111111111111111111111111111111");
                var roster2Id = Guid.Parse("22222222222222222222222222222222");
                var roster3Id = Guid.Parse("33333333333333333333333333333333");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.MultyOptionsQuestion(rosterSizeQuestionId, variable: "multi", yesNo: true, options: new List<Answer>
                    {
                        Create.Option(value: "20", text: "A"),
                        Create.Option(value: "30", text: "B")
                    }),

                    Create.Roster(
                        id: roster1Id,
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: rosterSizeQuestionId,
                        variable: "first",
                        children: new IComposite[]
                        {
                            Create.Roster(
                                id: roster2Id,
                                rosterSizeSourceType: RosterSizeSourceType.Question,
                                rosterSizeQuestionId: rosterSizeQuestionId,
                                variable: "second",
                                children: new IComposite[]
                                {
                                    Create.Roster(
                                        id: roster3Id,
                                        rosterSizeSourceType: RosterSizeSourceType.Question,
                                        rosterSizeQuestionId: rosterSizeQuestionId,
                                        variable: "third",
                                        children: new IComposite[]
                                        {

                                        })
                                })
                        })
                    );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument, new List<object>() { });

                interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(rosterSizeQuestionId, RosterVector.Empty, Yes(30)));

                using (var eventContext = new EventContext())
                {
                    interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(rosterSizeQuestionId, RosterVector.Empty, Yes(20)));

                    var deletedRosters = eventContext.GetSingleEvent<RosterInstancesRemoved>().Instances;
                    var addedRosters = eventContext.GetSingleEvent<RosterInstancesAdded>().Instances;

                    result.CountOfAddedInstances = addedRosters.Length;
                    result.CountOfRemovedInstances = deletedRosters.Length;

                    result.Roster1_20_Added = addedRosters.Any(x => x.RosterInstanceId == 20 && x.GroupId == roster1Id);
                    result.Roster2_20_20_Added = addedRosters.Any(x => x.RosterInstanceId == 20 && x.GroupId == roster2Id);
                    result.Roster3_20_20_20_Added = addedRosters.Any(x => x.RosterInstanceId == 20 && x.GroupId == roster3Id);

                    result.Roster1_30_Removed = deletedRosters.Any(x => x.RosterInstanceId == 30 && x.GroupId == roster1Id);
                    result.Roster2_30_30_Removed = deletedRosters.Any(x => x.RosterInstanceId == 30 && x.GroupId == roster2Id);
                    result.Roster3_30_30_30_Removed = deletedRosters.Any(x => x.RosterInstanceId == 30 && x.GroupId == roster3Id);
                }

                return result;
            });

        It should_add_3_rosters = () =>
            results.CountOfAddedInstances.ShouldEqual(3);

        It should_add_first_level_roster_with_intstance_is_20 = () =>
            results.Roster1_20_Added.ShouldBeTrue();

        It should_add_second_level_roster_with_intstance_is_20 = () =>
           results.Roster2_20_20_Added.ShouldBeTrue();

        It should_add_third_level_roster_with_intstance_is_20 = () =>
           results.Roster3_20_20_20_Added.ShouldBeTrue();

        It should_remove_3_rosters = () =>
            results.CountOfRemovedInstances.ShouldEqual(3);

        It should_remove_first_level_roster_with_intstance_is_30 = () =>
            results.Roster1_30_Removed.ShouldBeTrue();

        It should_remove_second_level_roster_with_intstance_is_30 = () =>
           results.Roster2_30_30_Removed.ShouldBeTrue();

        It should_remove_third_level_roster_with_intstance_is_30 = () =>
           results.Roster3_30_30_30_Removed.ShouldBeTrue();


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
            public bool Roster1_20_Added;
            public bool Roster2_20_20_Added;
            public bool Roster3_20_20_20_Added;
            public bool Roster1_30_Removed;
            public bool Roster2_30_30_Removed;
            public bool Roster3_30_30_30_Removed;
            public int CountOfAddedInstances { get; set; }
            public int CountOfRemovedInstances { get; set; }
        }
    }
}