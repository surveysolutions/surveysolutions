using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Aggregates
{
    internal class when_finding_referenced_answers_for_linked_question_on_roster_level_1_and_referenced_answers_are_on_roster_level_1 : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionRosterVector = new[] { 1m };
            var linkedQuestionRosters = new[] { referencedRoster1 };

            var referencedQuestionRosters = new[] { referencedRoster1 };

            SetupQuestionnaireWithLinkedAndReferencedQuestions(
                questionnaireId, linkedQuestionId, linkedQuestionRosters, referencedQuestionId, referencedQuestionRosters);

            interview = Create.StatefulInterview(questionnaireId: questionnaireId);

            FillInterviewWithInstancesForOneRosterAndAnswersToTextQuestionInThatRoster(interview, referencedRoster1, referencedQuestionId);
        };

        Because of = () =>
            result = interview.FindAnswersOfReferencedQuestionForLinkedQuestion(referencedQuestionId, Create.Identity(linkedQuestionId, linkedQuestionRosterVector));

        It should_return_all_answers = () =>
            result.Cast<TextAnswer>().Select(answer => answer.Answer)
                .ShouldContainOnly("1", "2");

        private static StatefulInterview interview;
        private static IEnumerable<BaseInterviewAnswer> result;
        private static Guid referencedQuestionId = Guid.Parse("55555555555555555555555555555555");
        private static Guid linkedQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] linkedQuestionRosterVector;
        private static Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid referencedRoster1 = Guid.Parse("00000000000000001111111111111111");
    }
}