using System;
using System.Linq.Expressions;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_determining_has_interview_a_group_and_group_is_in_roster_which_has_no_instances : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            IQuestionnaireStorage questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.HasGroup(@group.Id) == true
                && _.GetRostersFromTopToSpecifiedGroup(@group.Id) == new [] { rosterId });

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            result = interview.HasGroup(group);

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static StatefulInterview interview;
        private static Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterId = Guid.Parse("44444444444444444444444444444444");
        static Identity group = new Identity(Guid.Parse("11111111111111111111111111111111"), new [] { 4444m });
    }
}