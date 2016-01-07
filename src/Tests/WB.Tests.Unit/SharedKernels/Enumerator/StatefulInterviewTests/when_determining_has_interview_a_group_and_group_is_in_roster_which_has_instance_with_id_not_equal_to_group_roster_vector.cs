using System;
using System.Linq.Expressions;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_determining_has_interview_a_group_and_group_is_in_roster_which_has_instance_with_id_not_equal_to_group_roster_vector : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            IPlainQuestionnaireRepository questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.HasGroup(@group.Id) == true
                && _.GetRostersFromTopToSpecifiedGroup(@group.Id) == new Guid[] { rosterId });

            interview = Create.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.Apply(Create.Event.RosterInstancesAdded(rosterId, new[] { rosterInstanceId }));
        };

        Because of = () =>
            result = interview.HasGroup(group);

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static StatefulInterview interview;
        private static Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterId = Guid.Parse("44444444444444444444444444444444");
        private static decimal rosterInstanceId = 4444m;
        private static decimal incorrectRosterInstanceId = 3333m;
        static Identity group = new Identity(Guid.Parse("11111111111111111111111111111111"), new[] { incorrectRosterInstanceId });
    }
}