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
    internal class when_3_level_nested_rosters_are_triggered_by_same_yesno_question_and_answer_is_has_1_option : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var roster1Id = Guid.Parse("11111111111111111111111111111111");
                var roster2Id = Guid.Parse("22222222222222222222222222222222");
                var roster3Id = Guid.Parse("33333333333333333333333333333333");

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.MultyOptionsQuestion(rosterSizeQuestionId, variable: "multi", yesNoView: true, options: new List<Answer>
                    {
                        Create.Entity.Option(value: "20", text: "A"),
                        Create.Entity.Option(value: "30", text: "B")
                    }),

                    Create.Entity.Roster(
                        rosterId: roster1Id,
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: rosterSizeQuestionId,
                        variable: "first",
                        children: new IComposite[]
                        {
                            Create.Entity.Roster(
                                rosterId: roster2Id,
                                rosterSizeSourceType: RosterSizeSourceType.Question,
                                rosterSizeQuestionId: rosterSizeQuestionId,
                                variable: "second",
                                children: new IComposite[]
                                {
                                    Create.Entity.Roster(
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

                var interview = SetupInterview(questionnaireDocument, new List<object>());

                interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(questionId: rosterSizeQuestionId, answeredOptions: new [] { Yes(30) }));

                using (var eventContext = new EventContext())
                {
                    interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(questionId: rosterSizeQuestionId, answeredOptions: new[] { Yes(20) }));

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

        [NUnit.Framework.Test] public void should_add_3_rosters () =>
            results.CountOfAddedInstances.Should().Be(3);

        [NUnit.Framework.Test] public void should_add_first_level_roster_with_intstance_is_20 () =>
            results.Roster1_20_Added.Should().BeTrue();

        [NUnit.Framework.Test] public void should_add_second_level_roster_with_intstance_is_20 () =>
           results.Roster2_20_20_Added.Should().BeTrue();

        [NUnit.Framework.Test] public void should_add_third_level_roster_with_intstance_is_20 () =>
           results.Roster3_20_20_20_Added.Should().BeTrue();

        [NUnit.Framework.Test] public void should_remove_3_rosters () =>
            results.CountOfRemovedInstances.Should().Be(3);

        [NUnit.Framework.Test] public void should_remove_first_level_roster_with_intstance_is_30 () =>
            results.Roster1_30_Removed.Should().BeTrue();

        [NUnit.Framework.Test] public void should_remove_second_level_roster_with_intstance_is_30 () =>
           results.Roster2_30_30_Removed.Should().BeTrue();

        [NUnit.Framework.Test] public void should_remove_third_level_roster_with_intstance_is_30 () =>
           results.Roster3_30_30_30_Removed.Should().BeTrue();


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
