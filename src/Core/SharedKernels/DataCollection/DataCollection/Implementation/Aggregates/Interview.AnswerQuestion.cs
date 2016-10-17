using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        private InterviewChanges CalculateInterviewChangesOnAnswerQuestion(Guid userId, Guid questionId, RosterVector rosterVector,
            object answer, AnswerChangeType answerChangeType, DateTime answerTime, IQuestionnaire questionnaire,
            ILatestInterviewExpressionState expressionProcessorState)
        {
            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(answerChangeType, userId, questionId, rosterVector, answerTime, answer)
            };

            return this.CalculateInterviewStructuralChanges(questionId, rosterVector, answerTime, userId, questionnaire, expressionProcessorState,
                interviewByAnswerChange, null, this.interviewState);
        }

        private InterviewChanges CalculateInterviewStructuralChanges(Guid questionId, RosterVector rosterVector, DateTime answerTime, Guid userId,
            IQuestionnaire questionnaire, ILatestInterviewExpressionState expressionProcessorState, List<AnswerChange> interviewByAnswerChange,
            RosterCalculationData rosterCalculationData, IReadOnlyInterviewStateDependentOnAnswers alteredState)
        {
            var interviewChanges = this.EmitInterviewStructuralChangesByExpressionStateRosterTitlesAndLinked(expressionProcessorState,
                    rosterCalculationData, questionnaire, interviewByAnswerChange, userId, answerTime, questionId);

            var substitutedQuestionIds = questionnaire.GetSubstitutedQuestions(questionId);

            interviewChanges.ChangedQuestionTitles = this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(alteredState,
                substitutedQuestionIds, rosterVector, questionnaire).ToArray();

            var substitutedStaticTextIds = questionnaire.GetSubstitutedStaticTexts(questionId);

            interviewChanges.ChangedStaticTextTitles = this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(alteredState,
                substitutedStaticTextIds, rosterVector, questionnaire).ToArray();

            var substitutedGroupIds = questionnaire.GetSubstitutedGroups(questionId);

            interviewChanges.ChangedGroupTitles = this.GetInstancesOfGroupsWithSameAndDeeperRosterLevelOrThrow(alteredState,
                substitutedGroupIds, rosterVector, questionnaire).ToArray();

            return interviewChanges;
        }
    }
}