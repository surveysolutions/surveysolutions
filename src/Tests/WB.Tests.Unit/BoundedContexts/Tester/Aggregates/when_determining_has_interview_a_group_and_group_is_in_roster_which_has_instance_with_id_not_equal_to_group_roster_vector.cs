using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Aggregates
{
    internal class when_determining_has_interview_a_group_and_group_is_in_roster_which_has_instance_with_id_not_equal_to_group_roster_vector : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            Setup.QuestionnaireWithRepositoryToMockedServiceLocator(questionnaireId, _
                => _.HasGroup(group.Id) == true
                   && _.GetRostersFromTopToSpecifiedGroup(group.Id) == new Guid[] { rosterId });

            interview = Create.StatefulInterview(questionnaireId: questionnaireId);

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