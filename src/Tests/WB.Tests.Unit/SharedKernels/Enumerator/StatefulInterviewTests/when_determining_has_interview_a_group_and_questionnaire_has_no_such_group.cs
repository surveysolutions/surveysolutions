using System;
using FluentAssertions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_determining_has_interview_a_group_and_questionnaire_has_no_such_group : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            IQuestionnaireStorage questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.HasGroup(@group.Id) == false);

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
        static Identity group = Create.Identity(Guid.Parse("11111111111111111111111111111111"));
    }
}
