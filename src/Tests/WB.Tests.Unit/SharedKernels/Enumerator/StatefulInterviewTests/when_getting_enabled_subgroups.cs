using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [Ignore("KP-8159")]
    internal class when_getting_enabled_subgroups : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.QuestionnaireDocument(
                id: questionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.Group(
                        groupId: selectedGroupIdentity.Id,
                        children: new IComposite[]
                        {
                            Create.Entity.Roster(rosterId: rosterId),
                            Create.Entity.Group(groupId: groupId)
                        })
                });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository);

            statefulInterview.Apply(Create.Event.RosterInstancesAdded(rosterGroupId: rosterId,
                rosterInstanceId: rosterInstance2Id, sortIndex: 1));
            statefulInterview.Apply(Create.Event.RosterInstancesAdded(rosterGroupId: rosterId,
                rosterInstanceId: rosterInstance1Id, sortIndex: 2));
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