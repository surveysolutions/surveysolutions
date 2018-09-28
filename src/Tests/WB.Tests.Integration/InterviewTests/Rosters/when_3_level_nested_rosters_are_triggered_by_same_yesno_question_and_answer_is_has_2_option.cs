using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_3_level_nested_rosters_are_triggered_by_same_yesno_question_and_answer_is_has_2_option : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var roster1Id = Guid.Parse("11111111111111111111111111111111");
                var roster2Id = Guid.Parse("22222222222222222222222222222222");
                var roster3Id = Guid.Parse("33333333333333333333333333333333");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.MultyOptionsQuestion(rosterSizeQuestionId, variable: "multi", yesNoView: true, options: new List<Answer>
                    {
                        Abc.Create.Entity.Option(value: "10", text: "A"),
                        Abc.Create.Entity.Option(value: "20", text: "B"),
                        Abc.Create.Entity.Option(value: "30", text: "C")
                    }),

                    Abc.Create.Entity.Roster(
                        rosterId: roster1Id,
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: rosterSizeQuestionId,
                        variable: "first",
                        children: new IComposite[]
                        {
                            Abc.Create.Entity.Roster(
                                rosterId: roster2Id,
                                rosterSizeSourceType: RosterSizeSourceType.Question,
                                rosterSizeQuestionId: rosterSizeQuestionId,
                                variable: "second",
                                children: new IComposite[]
                                {
                                    Abc.Create.Entity.Roster(
                                        rosterId: roster3Id,
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

                interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(questionId: rosterSizeQuestionId, 
                    answeredOptions: new [] { Yes(30), Yes(10) }));

                using (var eventContext = new EventContext())
                {
                    interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(questionId: rosterSizeQuestionId,
                        answeredOptions: new[] { Yes(10), Yes(20) }));

                    var deletedRosters = eventContext.GetSingleEvent<RosterInstancesRemoved>().Instances;
                    var addedRosters = eventContext.GetSingleEvent<RosterInstancesAdded>().Instances;

                    result.CountOfAddedInstances = addedRosters.Length;
                    result.CountOfRemovedInstances = deletedRosters.Length;

                    result.DeletedRostersHas_30_RosterVector = deletedRosters.All(x => x.OuterRosterVector.Contains(30) || x.RosterInstanceId == 30);
                    result.AddedRostersHasNo_30_RosterVector = addedRosters.All(x => !x.OuterRosterVector.Contains(30) && x.RosterInstanceId != 30);
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_add_11_rosters () =>
            results.CountOfAddedInstances.Should().Be(11);

        [NUnit.Framework.Test] public void should_remove_11_rosters () =>
            results.CountOfRemovedInstances.Should().Be(11);

        [NUnit.Framework.Test] public void should_add_roster_that_roster_vector_has_no_deleted_instance_id_30 () =>
            results.AddedRostersHasNo_30_RosterVector.Should().BeTrue();

        [NUnit.Framework.Test] public void should_remove_roster_that_roster_vector_has_30 () =>
            results.DeletedRostersHas_30_RosterVector.Should().BeTrue();

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
            public bool DeletedRostersHas_30_RosterVector;
            public bool AddedRostersHasNo_30_RosterVector;
            public int CountOfAddedInstances { get; set; }
            public int CountOfRemovedInstances { get; set; }
        }
    }
}
