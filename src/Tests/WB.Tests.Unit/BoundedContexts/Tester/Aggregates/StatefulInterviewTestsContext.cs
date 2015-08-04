using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Tester.Implementation.Aggregates;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.Aggregates
{
    [Subject(typeof(StatefulInterview))]
    public class StatefulInterviewTestsContext
    {
        protected static void SetupQuestionnaireWithLinkedAndReferencedQuestions(Guid questionnaireId,
            Guid linkedQuestionId, Guid[] linkedQuestionRosters, Guid referencedQuestionId, Guid[] referencedQuestionRosters)
        {
            Setup.QuestionnaireWithRepositoryToMockedServiceLocator(questionnaireId, _
                => _.HasQuestion(linkedQuestionId) == true
                && _.GetRosterLevelForQuestion(linkedQuestionId) == linkedQuestionRosters.Length
                && _.GetRostersFromTopToSpecifiedQuestion(linkedQuestionId) == linkedQuestionRosters
                && _.GetRosterSizeSourcesForQuestion(linkedQuestionId) == linkedQuestionRosters
                && _.HasQuestion(referencedQuestionId) == true
                && _.GetRosterLevelForQuestion(referencedQuestionId) == referencedQuestionRosters.Length
                && _.GetRostersFromTopToSpecifiedQuestion(referencedQuestionId) == referencedQuestionRosters
                && _.GetRosterSizeSourcesForQuestion(referencedQuestionId) == referencedQuestionRosters);
        }

        protected static void FillInterviewWithInstancesForOneRosterAndAnswersToTextQuestionInThatRoster(StatefulInterview interview, Guid rosterId, Guid questionId)
        {
            interview.Apply(Create.Event.RosterInstancesAdded(rosterId,
                new[] { 1m },
                new[] { 2m }));

            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 1m }, "1"));
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 2m }, "2"));
        }

        protected static void FillInterviewWithInstancesForTwoNestedRostersAndAnswersToTextQuestionInLastRoster(StatefulInterview interview, Guid roster1Id, Guid roster2Id, Guid questionId)
        {
            interview.Apply(Create.Event.RosterInstancesAdded(roster1Id,
                new[] { 1m },
                new[] { 2m }));

            interview.Apply(Create.Event.RosterInstancesAdded(roster2Id,
                new[] { 1m, 1m },
                new[] { 1m, 2m },
                new[] { 2m, 1m },
                new[] { 2m, 2m }));

            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 1m, 1m }, "1-1"));
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 1m, 2m }, "1-2"));
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 2m, 1m }, "2-1"));
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 2m, 2m }, "2-2"));
        }

        protected static void FillInterviewWithInstancesForThreeNestedRostersAndAnswersToTextQuestionInLastRoster(StatefulInterview interview, Guid roster1Id, Guid roster2Id, Guid roster3Id, Guid questionId)
        {
            interview.Apply(Create.Event.RosterInstancesAdded(roster1Id,
                new[] { 1m },
                new[] { 2m }));

            interview.Apply(Create.Event.RosterInstancesAdded(roster2Id,
                new[] { 1m, 1m },
                new[] { 1m, 2m },
                new[] { 2m, 1m },
                new[] { 2m, 2m }));

            interview.Apply(Create.Event.RosterInstancesAdded(roster3Id,
                new[] { 1m, 1m, 1m },
                new[] { 1m, 1m, 2m },
                new[] { 1m, 2m, 1m },
                new[] { 1m, 2m, 2m },
                new[] { 2m, 1m, 1m },
                new[] { 2m, 1m, 2m },
                new[] { 2m, 2m, 1m },
                new[] { 2m, 2m, 2m }));

            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 1m, 1m, 1m }, "1-1-1"));
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 1m, 1m, 2m }, "1-1-2"));
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 1m, 2m, 1m }, "1-2-1"));
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 1m, 2m, 2m }, "1-2-2"));
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 2m, 1m, 1m }, "2-1-1"));
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 2m, 1m, 2m }, "2-1-2"));
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 2m, 2m, 1m }, "2-2-1"));
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 2m, 2m, 2m }, "2-2-2"));
        }
    }
}