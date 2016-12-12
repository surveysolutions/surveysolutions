using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_determining_has_interview_a_group_and_questionnaire_has_such_group_and_group_is_not_in_roster : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(new[]
            {
                Create.Entity.Group(@group.Id)
            });

            interview = Setup.StatefulInterview(questionnaire);
        };

        Because of = () => result = interview.HasGroup(group);

        It should_return_true = () => result.ShouldBeTrue();

        private static bool result;
        private static StatefulInterview interview;
        static Identity group = new Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[] { });
    }
}