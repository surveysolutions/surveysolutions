using System.Collections.Generic;

namespace WB.Core.Infrastructure.BaseStructures
{
    public interface IExpressionProcessor
    {
        void ProcessValidationExpressions(IInterviewExpressionState interviewExpressionState, List<Identity> questionsToBeValid,
            List<Identity> questionsToBeInvalid);

        void ProcessConditionExpressions(IInterviewExpressionState interviewExpressionState,
            List<Identity> enabledGroups,
            List<Identity> disabledGroups,
            List<Identity> enabledQuestions,
            List<Identity> disabledQuestions);
    }
}