using System.Collections.Generic;
using WB.Core.Infrastructure.BaseStructures;

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    public interface IExpressionProcessor
    {
        void ProcessValidationExpressions(IInterviewExpressionState interviewExpressionState, out List<Identity> questionsToBeValid,
            out List<Identity> questionsToBeInvalid);

        void ProcessConditionExpressions(IInterviewExpressionState interviewExpressionState,
            List<Identity> enabledGroups,
            List<Identity> disabledGroups,
            List<Identity> enabledQuestions,
            List<Identity> disabledQuestions);
    }
}