using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Aggregates
{
    internal class when_finding_referenced_answers_for_linked_question_on_roster_level_0 : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            linkedQuestionRosterVector = new decimal[] { };

            var referencedRoster1 = Guid.Parse("00000000000000001111111111111111");
            var referencedRoster2 = Guid.Parse("00000000000000002222222222222222");

            var linkedQuestionRosters = new Guid[] { };
            var referencedQuestionRosters = new[] { referencedRoster1, referencedRoster2 };

            SetupQuestionnaireWithLinkedAndReferencedQuestions(
                questionnaireId, linkedQuestionId, linkedQuestionRosters, referencedQuestionId, referencedQuestionRosters);

            interview = Create.StatefulInterview(questionnaireId: questionnaireId);

            interview.Apply(Create.Event.RosterInstancesAdded(referencedRoster1,
                new[] { 1m },
                new[] { 2m }));

            interview.Apply(Create.Event.RosterInstancesAdded(referencedRoster2,
                new[] { 1m, 1m },
                new[] { 1m, 2m },
                new[] { 2m, 1m },
                new[] { 2m, 2m }));

            interview.Apply(Create.Event.TextQuestionAnswered(referencedQuestionId, new[] { 1m, 1m }, "1-1"));
            interview.Apply(Create.Event.TextQuestionAnswered(referencedQuestionId, new[] { 1m, 2m }, "1-2"));
            interview.Apply(Create.Event.TextQuestionAnswered(referencedQuestionId, new[] { 2m, 1m }, "2-1"));
            interview.Apply(Create.Event.TextQuestionAnswered(referencedQuestionId, new[] { 2m, 2m }, "2-2"));
        };

        Because of = () =>
            result = interview.FindAnswersOfLinkedToQuestionForLinkedQuestion(referencedQuestionId, Create.Identity(linkedQuestionId, linkedQuestionRosterVector));

        It should_return_all_answers = () =>
            result.Cast<TextAnswer>().Select(answer => answer.Answer)
                .ShouldContainOnly("1-1", "1-2", "2-1", "2-2");

        private static StatefulInterview interview;
        private static IEnumerable<BaseInterviewAnswer> result;
        private static Guid referencedQuestionId = Guid.Parse("55555555555555555555555555555555");
        private static Guid linkedQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] linkedQuestionRosterVector;
        private static Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}