using System;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewInfrastructure
{
    internal class InterviewStateFull : Interview
    {
        public Guid QuestionaryId
        {
            get { return base.questionnaireId; }
        }

        public object GetAnswerOnQuestion(Guid questionId, decimal[] rosterVector = null)
        {
            string questionKey = ConversionHelper.ConvertIdAndRosterVectorToString(questionId, rosterVector);
            return this.interviewState.AnswersSupportedInExpressions[questionKey];
        }
    }
}