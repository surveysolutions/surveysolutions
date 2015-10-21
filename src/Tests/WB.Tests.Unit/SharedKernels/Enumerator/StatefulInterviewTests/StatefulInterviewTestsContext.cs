using System;
using System.Linq.Expressions;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [Subject(typeof(StatefulInterview))]
    internal class StatefulInterviewTestsContext
    {
        protected static AnsweredQuestionSynchronizationDto CreateAnsweredQuestionSynchronizationDto(Guid questionId, decimal[] rosterVector, object answer, string comment = "comment")
        {
            return new AnsweredQuestionSynchronizationDto(questionId, rosterVector, answer, comment);
        }

        protected static void FillInterviewWithInstancesForOneRosterAndAnswersToTextQuestionInThatRoster(StatefulInterview interview, Guid rosterId, Guid questionId)
        {
            interview.Apply(Create.Event.RosterInstancesAdded(rosterId,
                new[] { 1m },
                new[] { 2m }));

            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 1m }, "1"));
            interview.Apply(Create.Event.TextQuestionAnswered(questionId, new[] { 2m }, "2"));
        }

        protected static IQuestionnaireRepository CreateQuestionnaireRepositoryStubWithOneQuestionnaire(Guid questionnaireId, IQuestionnaire questionaire = null)
        {
            return Create.QuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionaire);
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