using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerMultipleOptionsQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal[] selectedValues)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);
            this.CheckMultipleOptionQuestionInvariants(questionId, rosterVector, selectedValues, questionnaire, answeredQuestion,
                this.interviewState);

            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer =
                (currentState, question) => question == answeredQuestion
                    ? selectedValues
                    : this.GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            var expressionProcessorState = this.GetClonedExpressionState();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerMultipleOptionsQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, selectedValues, getAnswer, questionnaire);

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            this.ApplyInterviewChanges(interviewChanges);
            this.ApplyValidityChangesEvents(validationChanges);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerMultipleOptionsQuestion(ILatestInterviewExpressionState expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state,
            Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime,
            decimal[] selectedValues, Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer,
            IQuestionnaire questionnaire)
        {
            List<decimal> availableValues = questionnaire.GetMultiSelectAnswerOptionsAsValues(questionId).ToList();

            IEnumerable<decimal> rosterInstanceIds = selectedValues.ToList();

            Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes;
            if (!questionnaire.ShouldQuestionRecordAnswersOrder(questionId))
            {
                rosterInstanceIdsWithSortIndexes = selectedValues.ToDictionary(
                    selectedValue => selectedValue,
                    selectedValue => (int?)availableValues.IndexOf(selectedValue));
            }
            else
            {
                int? orderPosition = 0;
                rosterInstanceIdsWithSortIndexes = rosterInstanceIds.ToDictionary(
                    selectedValue => selectedValue,
                    selectedValue => orderPosition++);
            }

            var interviewByAnswerChange = new List<AnswerChange>
            {
                new AnswerChange(AnswerChangeType.MultipleOptions, userId, questionId, rosterVector, answerTime, selectedValues)
            };

            expressionProcessorState.UpdateMultiOptionAnswer(questionId, rosterVector, selectedValues);

            return this.EmitInterviewChangesForMultioptionQuestion(questionId, rosterVector, answerTime, userId, questionnaire, expressionProcessorState, state, getAnswer,
                rosterInstanceIdsWithSortIndexes, interviewByAnswerChange, rosterInstanceIds);
        }

        private InterviewChanges EmitInterviewChangesForMultioptionQuestion(Guid questionId, RosterVector rosterVector, DateTime answerTime, Guid userId,
            IQuestionnaire questionnaire, ILatestInterviewExpressionState expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state,
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer, Dictionary<decimal, int?> rosterInstanceIdsWithSortIndexes,
            List<AnswerChange> interviewByAnswerChange, IEnumerable<decimal> rosterInstanceIds)
        {
            List<Guid> rosterIds = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToList();
            Func<Guid, decimal[], bool> isRoster = (groupId, groupOuterRosterVector)
                => rosterIds.Contains(groupId)
                   && AreEqualRosterVectors(groupOuterRosterVector, rosterVector);

            RosterCalculationData rosterCalculationData = this.CalculateRosterDataWithRosterTitlesFromYesNoQuestions(
                questionId, rosterVector, rosterIds, rosterInstanceIdsWithSortIndexes, questionnaire, state, getAnswer);

            var rosterInstancesToAdd =
                this.GetUnionOfUniqueRosterInstancesToAddWithRosterTitlesByRosterAndNestedRosters(rosterCalculationData);

            var rosterInstancesToRemove = this.GetOrderedUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                d => d.RosterInstancesToRemove, new RosterIdentityComparer(), rosterCalculationData);

            foreach (var rosterInstanceToAdd in rosterInstancesToAdd)
            {
                expressionProcessorState.AddRoster(rosterInstanceToAdd.Key.GroupId,
                    rosterInstanceToAdd.Key.OuterRosterVector, rosterInstanceToAdd.Key.RosterInstanceId,
                    rosterInstanceToAdd.Key.SortIndex);
            }
            rosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));

            IReadOnlyInterviewStateDependentOnAnswers alteredState = state.Amend(getRosterInstanceIds: (groupId, groupOuterRosterVector)
                    => isRoster(groupId, groupOuterRosterVector)
                        ? rosterInstanceIds
                        : state.GetRosterInstanceIds(groupId, groupOuterRosterVector));

            return this.CalculateInterviewStructuralChanges(questionId, rosterVector, answerTime, userId, questionnaire, expressionProcessorState,
                interviewByAnswerChange, rosterCalculationData, alteredState);
        }
    }
}