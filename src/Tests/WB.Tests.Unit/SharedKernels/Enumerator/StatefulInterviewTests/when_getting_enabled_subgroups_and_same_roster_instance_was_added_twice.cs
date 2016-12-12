using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_enabled_subgroups_and_same_roster_instance_was_added_twice : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                id: questionnaireId,
                chapterId: selectedGroupIdentity.Id,
                children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: questionId),
                    Create.Entity.Roster(rosterId, rosterSizeQuestionId: questionId),
                });

            statefulInterview = Setup.StatefulInterview(questionnaire);
        };

        Because of = () =>
        {
            statefulInterview.Apply(Create.Event.RosterInstancesAdded(rosterId, Create.Entity.RosterVector(rosterInstance1Id)));
            statefulInterview.Apply(Create.Event.RosterInstancesAdded(rosterId, Create.Entity.RosterVector(rosterInstance1Id)));
            enabledSubgroupsIdentities = statefulInterview.GetEnabledSubgroups(selectedGroupIdentity).ToArray();
        };

        It should_contain_1_identity = () =>
            enabledSubgroupsIdentities.Length.ShouldEqual(1);

        static StatefulInterview statefulInterview;
        static Identity[] enabledSubgroupsIdentities;
        static readonly Identity selectedGroupIdentity = new Identity(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);
        static readonly Guid questionId = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111112");
        static readonly Guid rosterId = Guid.Parse("11111111111111111111111111111116");
        const decimal rosterInstance1Id = 4444m;
    }
}