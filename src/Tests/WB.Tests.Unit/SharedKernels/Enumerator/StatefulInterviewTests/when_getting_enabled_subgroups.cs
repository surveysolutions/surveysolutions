using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_enabled_subgroups : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            IQuestionnaireRepository questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.GetAllUnderlyingChildGroupsAndRosters(selectedGroupIdentity.Id) == new ReadOnlyCollection<Guid>(new[] { rosterId, groupId })
                && _.IsRosterGroup(rosterId) == true
                && _.IsRosterGroup(groupId) == false
                && _.IsQuestion(groupId) == false
                && _.IsQuestion(rosterId) == false
                && _.IsInterviewierQuestion(groupId) == false
                && _.IsInterviewierQuestion(rosterId) == false
                && _.GetRosterLevelForEntity(groupId) == 0
                && _.GetRosterLevelForGroup(rosterId) == 1
                && _.GetRostersFromTopToSpecifiedGroup(rosterId) == new[] { rosterId }
                && _.GetRostersFromTopToSpecifiedEntity(groupId) == new Guid[] { });

            statefulInterview = Create.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            statefulInterview.Apply(Create.Event.RosterInstancesAdded(rosterGroupId: rosterId, rosterInstanceId: rosterInstance2Id, sortIndex: 1));
            statefulInterview.Apply(Create.Event.RosterInstancesAdded(rosterGroupId: rosterId, rosterInstanceId: rosterInstance1Id, sortIndex: 2));
        };

        Because of = () =>
        {
            enabledSubgroupsIds =
                statefulInterview.GetEnabledSubgroups(selectedGroupIdentity).ToArray();
        };

        It should_contains_3_identities = () =>
            enabledSubgroupsIds.Count().ShouldEqual(3);

        It should_first_identity_be_rosterInstance2Id_instance_of_rosterId = () =>
            enabledSubgroupsIds[0].ShouldEqual(new Identity(rosterId, new[]{rosterInstance2Id}));

        It should_second_identity_be_rosterInstance1Id_instance_of_rosterId = () =>
            enabledSubgroupsIds[1].ShouldEqual(new Identity(rosterId, new[] { rosterInstance1Id }));

        It should_third_identity_be_groupId = () =>
          enabledSubgroupsIds[2].ShouldEqual(new Identity(groupId, new decimal[0]));

        static StatefulInterview statefulInterview;
        static readonly Identity selectedGroupIdentity = new Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[0]);
        static Identity[] enabledSubgroupsIds;
        static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111112");
        static readonly Guid rosterId = Guid.Parse("11111111111111111111111111111116");
        static readonly Guid groupId = Guid.Parse("11111111111111111111111111111117");
        const decimal rosterInstance1Id = 4444m;
        const decimal rosterInstance2Id = 555m;
    }
}