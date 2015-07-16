using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Aggregates
{
    internal class when_determining_has_interview_a_group_and_group_is_in_roster_which_has_no_instances : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            Setup.QuestionnaireWithRepositoryToMockedServiceLocator(questionnaireId, _
                => _.HasGroup(group.Id) == true
                && _.GetRostersFromTopToSpecifiedGroup(group.Id) == new [] { rosterId });

            interview = Create.StatefulInterview(questionnaireId: questionnaireId);
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