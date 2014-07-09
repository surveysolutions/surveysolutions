using System.Collections.Generic;

namespace WB.Core.Infrastructure.BaseStructures
{
    public class ExpressionProcessor : IExpressionProcessor
    {
        public void ProcessValidationExpressions(IInterviewExpressionState interviewExpressionState, List<Identity> questionsToBeValid,
            List<Identity> questionsToBeInvalid)
        {
            throw new System.NotImplementedException();
        }

        public void ProcessConditionExpressions(IInterviewExpressionState interviewExpressionState,
            List<Identity> enabledGroups,
            List<Identity> disabledGroups,
            List<Identity> enabledQuestions,
            List<Identity> disabledQuestions)
        {
            throw new System.NotImplementedException();
        }
    }
}