using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Aggregates
{
    internal class when_finding_referenced_answers_for_linked_question_on_roster_level_2_and_referenced_answers_are_on_roster_level_2_but_first_level_roster_differs : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionRosterVector = new[] { 1m, 1m };
            var linkedQuestionRosters = new[] { referencedRoster1B, referencedRoster2 };

            var referencedQuestionRosters = new[] { referencedRoster1A, referencedRoster2 };

            SetupQuestionnaireWithLinkedAndReferencedQuestions(
                questionnaireId, linkedQuestionId, linkedQuestionRosters, referencedQuestionId, referencedQuestionRosters);

            interview = Create.StatefulInterview(questionnaireId: questionnaireId);

            FillInterviewWithInstancesForTwoNestedRostersAndAnswersToTextQuestionInLastRoster(interview, referencedRoster1A, referencedRoster2, referencedQuestionId);
        };

        Because of = () =>
            result = interview.FindAnswersOfReferencedQuestionForLinkedQuestion(referencedQuestionId, Create.Identity(linkedQuestionId, linkedQuestionRosterVector));

        It should_return_all_answers = () =>
            result.Cast<TextAnswer>().Select(answer => answer.Answer)
                .ShouldContainOnly("1-1", "1-2", "2-1", "2-2");

        private static StatefulInterview interview;
        private static IEnumerable<BaseInterviewAnswer> result;
        private static Guid referencedQuestionId = Guid.Parse("55555555555555555555555555555555");
        private static Guid linkedQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] linkedQuestionRosterVector;
        private static Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid referencedRoster1A = Guid.Parse("000000000000000011111111AAAAAAAA");
        private static Guid referencedRoster1B = Guid.Parse("000000000000000011111111BBBBBBBB");
        private static Guid referencedRoster2 = Guid.Parse("00000000000000002222222222222222");
    }
}