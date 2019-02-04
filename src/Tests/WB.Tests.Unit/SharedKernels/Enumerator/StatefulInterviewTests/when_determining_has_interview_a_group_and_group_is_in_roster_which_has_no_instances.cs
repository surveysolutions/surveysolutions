using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_determining_has_interview_a_group_and_group_is_in_roster_which_has_no_instances : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            IQuestionnaireStorage questionnaireRepository = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.HasGroup(@group.Id) == true
                && _.GetRostersFromTopToSpecifiedGroup(@group.Id) == new [] { rosterId });

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            BecauseOf();
        }

        private void BecauseOf() =>
            result = interview.HasGroup(group);

        [NUnit.Framework.Test] public void should_return_false () =>
            result.Should().BeFalse();

        private static bool result;
        private static StatefulInterview interview;
        private static Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterId = Guid.Parse("44444444444444444444444444444444");
        static Identity group = Create.Identity(Guid.Parse("11111111111111111111111111111111"), 4444);
    }
}
