using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_group_in_roster : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new[]
            {
                Create.Entity.FixedRoster(fixedTitles: new[] {Create.Entity.FixedTitle(rosterInstanceId)}, children: new[]
                {
                    Create.Entity.Group(@group.Id)
                })
            });

            interview = Setup.StatefulInterview(questionnaire);
        };

        Because of = () =>
            result = interview.HasGroup(group);

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static StatefulInterview interview;
        private static int rosterInstanceId = 4444;
        static Identity group = Create.Identity(Guid.Parse("11111111111111111111111111111111"), rosterInstanceId);
    }
}