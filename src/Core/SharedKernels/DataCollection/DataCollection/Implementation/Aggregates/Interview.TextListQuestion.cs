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
        public void AnswerTextListQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime,
            Tuple<decimal, string>[] answers)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            CheckTextListInvariants(questionId, rosterVector, questionnaire, answeredQuestion, this.interviewState, answers);

            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer = (currentState, question) => question == answeredQuestion ?
                answers :
                this.GetEnabledQuestionAnswerSupportedInExpressions(this.interviewState, question, questionnaire);

            var expressionProcessorState = this.GetClonedExpressionState();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerTextListQuestion(expressionProcessorState, this.interviewState, userId, questionId,
                rosterVector, answerTime, answers, getAnswer, questionnaire);

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            this.ApplyInterviewChanges(interviewChanges);
            this.ApplyValidityChangesEvents(validationChanges);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerTextListQuestion(ILatestInterviewExpressionState expressionProcessorState, IReadOnlyInterviewStateDependentOnAnswers state,
            Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, Tuple<decimal, string>[] answers,
            Func<IReadOnlyInterviewStateDependentOnAnswers, Identity, object> getAnswer, IQuestionnaire questionnaire)
        {
            var selectedValues = answers.Select(x => x.Item1).ToArray();

            var rosterInstanceIdsWithSortIndexes = selectedValues.ToDictionary(
                selectedValue => selectedValue,
                selectedValue => (int?)selectedValue);

            List<Guid> rosterInstanceIds = questionnaire.GetRosterGroupsByRosterSizeQuestion(questionId).ToList();

            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(questionId, rosterVector);

            Tuple<decimal, string>[] currentAnswer = this.interviewState.TextListAnswers.ContainsKey(questionKey)
                ? this.interviewState.TextListAnswers[questionKey]
                : new Tuple<decimal, string>[0];

            Tuple<decimal, string>[] changedAnswers =
                answers.Where(tuple => currentAnswer.Any(a => a.Item1 == tuple.Item1 && a.Item2 != tuple.Item2)).ToArray();

            RosterCalculationData rosterCalculationData = this.CalculateRosterDataWithRosterTitlesFromTextListQuestions(
                state, rosterVector, rosterInstanceIds, rosterInstanceIdsWithSortIndexes, questionnaire, getAnswer,
                answers, changedAnswers);

            var interviewByAnswerChange = new List<AnswerChange>()
            {
                new AnswerChange(AnswerChangeType.TextList, userId, questionId, rosterVector, answerTime, answers)
            };

            expressionProcessorState.UpdateTextListAnswer(questionId, rosterVector, answers);

            var rosterInstancesToAdd = this.GetUnionOfUniqueRosterInstancesToAddWithRosterTitlesByRosterAndNestedRosters(rosterCalculationData);
            var rosterInstancesToRemove = this.GetOrderedUnionOfUniqueRosterDataPropertiesByRosterAndNestedRosters(
                d => d.RosterInstancesToRemove, new RosterIdentityComparer(), rosterCalculationData);

            foreach (var rosterInstanceToAdd in rosterInstancesToAdd)
            {
                expressionProcessorState.AddRoster(rosterInstanceToAdd.Key.GroupId,
                    rosterInstanceToAdd.Key.OuterRosterVector, rosterInstanceToAdd.Key.RosterInstanceId,
                    rosterInstanceToAdd.Key.SortIndex);
            }
            rosterInstancesToRemove.ForEach(r => expressionProcessorState.RemoveRoster(r.GroupId, r.OuterRosterVector, r.RosterInstanceId));

            return this.CalculateInterviewStructuralChanges(questionId, rosterVector, answerTime, userId, questionnaire, expressionProcessorState,
                interviewByAnswerChange, rosterCalculationData, state);
        }
    }
}