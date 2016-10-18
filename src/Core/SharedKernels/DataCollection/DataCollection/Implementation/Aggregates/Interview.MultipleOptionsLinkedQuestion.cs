using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    public partial class Interview
    {
        public void AnswerMultipleOptionsLinkedQuestion(Guid userId, Guid questionId, RosterVector rosterVector, DateTime answerTime, decimal[][] selectedRosterVectors)
        {
            new InterviewPropertiesInvariants(this.properties).RequireAnswerCanBeChanged();

            var answeredQuestion = new Identity(questionId, rosterVector);

            IQuestionnaire questionnaire = this.GetQuestionnaireOrThrow(this.questionnaireId, this.questionnaireVersion, this.language);

            this.CheckLinkedMultiOptionQuestionInvariants(questionId, rosterVector, selectedRosterVectors, questionnaire, answeredQuestion);

            ILatestInterviewExpressionState expressionProcessorState = this.GetClonedExpressionState();

            expressionProcessorState.UpdateLinkedMultiOptionAnswer(questionId, rosterVector, selectedRosterVectors);

            InterviewChanges interviewChanges = this.CalculateInterviewChangesOnAnswerQuestion(userId, questionId, rosterVector, selectedRosterVectors,
                AnswerChangeType.MultipleOptionsLinked, answerTime, questionnaire, expressionProcessorState);

            this.ApplyInterviewChanges(interviewChanges);
        }
    }
}