using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_enabled_subgroups : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                id: questionnaireId,
                chapterId: selectedGroupIdentity.Id,
                children: new IComposite[]
                {
                    Create.Entity.FixedRoster(rosterId, fixedTitles:
                        new[]
                        {
                            Create.Entity.FixedTitle(rosterInstance2Id, "first roster"),
                            Create.Entity.FixedTitle(rosterInstance1Id, "second roster")
                        }),
                    Create.Entity.Group(groupId: groupId)
                });

            statefulInterview = Setup.StatefulInterview(questionnaire);
        };

        Because of = () =>
        {
            enabledSubgroupsIds =
                statefulInterview.GetEnabledSubgroups(selectedGroupIdentity).ToArray();
        };

        It should_contains_3_identities = () =>
            enabledSubgroupsIds.ShouldContainOnly(new Identity(rosterId, new[] {rosterInstance2Id}),
                new Identity(rosterId, new[] {rosterInstance1Id}), new Identity(groupId, new decimal[0]));

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