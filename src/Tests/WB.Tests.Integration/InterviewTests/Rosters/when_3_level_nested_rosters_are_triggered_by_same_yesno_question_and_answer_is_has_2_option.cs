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
    internal class when_3_level_nested_rosters_are_triggered_by_same_yesno_question_and_answer_is_has_2_option : InterviewTestsContext
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
                        Create.Option(value: "10", text: "A"),
                        Create.Option(value: "20", text: "B"),
                        Create.Option(value: "30", text: "C")
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

                interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(rosterSizeQuestionId, RosterVector.Empty,
                    Yes(30), Yes(10)));

                using (var eventContext = new EventContext())
                {
                    interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(rosterSizeQuestionId, RosterVector.Empty,
                        Yes(10), Yes(20)));

                    var deletedRosters = eventContext.GetSingleEvent<RosterInstancesRemoved>().Instances;
                    var addedRosters = eventContext.GetSingleEvent<RosterInstancesAdded>().Instances;

                    result.CountOfAddedInstances = addedRosters.Length;
                    result.CountOfRemovedInstances = deletedRosters.Length;

                    result.DeletedRostersHas_30_RosterVector = deletedRosters.All(x => x.OuterRosterVector.Contains(30) || x.RosterInstanceId == 30);
                    result.AddedRostersHasNo_30_RosterVector = addedRosters.All(x => !x.OuterRosterVector.Contains(30) && x.RosterInstanceId != 30);
                }

                return result;
            });

        It should_add_11_rosters = () =>
            results.CountOfAddedInstances.ShouldEqual(11);

        It should_remove_11_rosters = () =>
            results.CountOfRemovedInstances.ShouldEqual(11);

        It should_add_roster_that_roster_vector_has_no_deleted_instance_id_30 = () =>
            results.AddedRostersHasNo_30_RosterVector.ShouldBeTrue();

        It should_remove_roster_that_roster_vector_has_30 = () =>
            results.DeletedRostersHas_30_RosterVector.ShouldBeTrue();

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
            public bool DeletedRostersHas_30_RosterVector;
            public bool AddedRostersHasNo_30_RosterVector;
            public int CountOfAddedInstances { get; set; }
            public int CountOfRemovedInstances { get; set; }
        }
    }
}