using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Moq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Snapshots;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Tests
{
    internal static class Create
    {
        public static Questionnaire Questionnaire(Guid creatorId, QuestionnaireDocument document)
        {
            return new Questionnaire(new Guid(), document, false, string.Empty);
        }

        public static EnablementChanges EnablementChanges(List<Identity> groupsToBeDisabled = null, List<Identity> groupsToBeEnabled = null,
            List<Identity> questionsToBeDisabled = null, List<Identity> questionsToBeEnabled = null)
        {
            return new EnablementChanges(
                groupsToBeDisabled ?? new List<Identity>(),
                groupsToBeEnabled ?? new List<Identity>(),
                questionsToBeDisabled ?? new List<Identity>(),
                questionsToBeEnabled ?? new List<Identity>());
        }

        public static InterviewState InterviewState(InterviewStatus? status =null,List<AnswerComment> answerComments = null)
        {
            return new InterviewState(Guid.NewGuid(), 1, status ?? InterviewStatus.SupervisorAssigned, new Dictionary<string, object>(),
                new Dictionary<string, Tuple<Guid, decimal[], decimal[]>>(), new Dictionary<string, Tuple<Guid, decimal[], decimal[][]>>(),
                new Dictionary<string, Tuple<decimal, string>[]>(), new HashSet<string>(),
                answerComments ?? new List<AnswerComment>(),
                new HashSet<string>(),
                new HashSet<string>(), new Dictionary<string, DistinctDecimalList>(),
                new HashSet<string>(), new HashSet<string>(), true, Mock.Of<IInterviewExpressionState>());
        }

        public static Identity Identity(Guid id, decimal[] rosterVector)
        {
            return new Identity(id, rosterVector);
        }

        public static IQuestionnaireRepository QuestionnaireRepositoryStubWithOneQuestionnaire(
            Guid questionnaireId, IQuestionnaire questionaire = null)
        {
            questionaire = questionaire ?? Mock.Of<IQuestionnaire>();

            return Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionaire
                && repository.GetHistoricalQuestionnaire(questionnaireId, questionaire.Version) == questionaire
                && repository.GetHistoricalQuestionnaire(questionnaireId, 1) == questionaire);
        }
    }
}