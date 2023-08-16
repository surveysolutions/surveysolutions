using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_enabled_subgroups : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
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

            statefulInterview = SetUp.StatefulInterview(questionnaire);
            BecauseOf();
        }

        private void BecauseOf() =>
            enabledSubgroupsIds = statefulInterview.GetEnabledSubgroupsAndRosters(selectedGroupIdentity).ToArray();

        [NUnit.Framework.Test] public void should_contains_3_identities () =>
            enabledSubgroupsIds.Should().BeEquivalentTo(new []{
                Create.Identity(rosterId, rosterInstance2Id),
                Create.Identity(rosterId, rosterInstance1Id), 
                Create.Identity(groupId, RosterVector.Empty)});

        static StatefulInterview statefulInterview;
        static readonly Identity selectedGroupIdentity = Create.Identity(Guid.Parse("11111111111111111111111111111111"), RosterVector.Empty);
        static Identity[] enabledSubgroupsIds;
        static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111112");
        static readonly Guid rosterId = Guid.Parse("11111111111111111111111111111116");
        static readonly Guid groupId = Guid.Parse("11111111111111111111111111111117");
        const int rosterInstance1Id = 4444;
        const int rosterInstance2Id = 555;
    }
}
