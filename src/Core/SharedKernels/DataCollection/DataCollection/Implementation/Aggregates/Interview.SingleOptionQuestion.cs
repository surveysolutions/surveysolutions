using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerSingleOptionQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal selectedValue)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);
            this.CheckSingleOptionQuestionInvariants(questionId, rosterVector, selectedValue, questionnaire, answeredQuestion,
                this.interviewState);

            var expressionProcessorState = this.GetClonedExpressionState();

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerSingleOptionQuestion(expressionProcessorState, this.interviewState, userId,
                questionId, rosterVector, answerTime, selectedValue, questionnaire);

            ValidityChanges validationChanges = expressionProcessorState.ProcessValidationExpressions();

            this.ApplyInterviewChanges(interviewChanges);
            this.ApplyValidityChangesEvents(validationChanges);
        }

        private InterviewChanges CalculateInterviewChangesOnAnswerSingleOptionQuestion(ILatestInterviewExpressionState expressionProcessorState,
            IReadOnlyInterviewStateDependentOnAnswers state, Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal selectedValue,
            IQuestionnaire questionnaire)
        {
            var questionIdentity = new Identity(questionId, rosterVector);
            var previsousAnswer = GetEnabledQuestionAnswerSupportedInExpressions(state, questionIdentity, questionnaire);
            bool answerChanged = state.WasQuestionAnswered(questionIdentity) && (decimal?)previsousAnswer != (decimal?)selectedValue;

            var answersToRemoveByCascading = answerChanged ? this.GetQuestionsToRemoveAnswersFromDependingOnCascading(questionId, rosterVector, questionnaire, state) : Enumerable.Empty<Identity>();
            var cascadingQuestionsToDisable = questionnaire.GetCascadingQuestionsThatDirectlyDependUponQuestion(questionId)
                .Where(question => !questionnaire.DoesCascadingQuestionHaveOptionsForParentValue(question, selectedValue)).ToList();
            var cascadingQuestionsToDisableIdentities = this.GetInstancesOfEntitiesWithSameAndDeeperRosterLevelOrThrow(state, cascadingQuestionsToDisable, rosterVector, questionnaire);

            expressionProcessorState.UpdateSingleOptionAnswer(questionId, rosterVector, selectedValue);
            answersToRemoveByCascading.ToList().ForEach(x => expressionProcessorState.UpdateSingleOptionAnswer(x.Id, x.RosterVector, (decimal?)null));
            expressionProcessorState.DisableQuestions(cascadingQuestionsToDisableIdentities);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQuestion(userId, questionId, rosterVector, selectedValue,
                AnswerChangeType.SingleOption, answerTime, questionnaire, expressionProcessorState);

            interviewChanges.AnswersToRemove.AddRange(answersToRemoveByCascading);
            return interviewChanges;
        }
    }
}