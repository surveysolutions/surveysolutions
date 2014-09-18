using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IExpressionProcessor
    {
        void ProcessValidationExpressions(IInterviewExpressionState interviewExpressionState, 
            out List<Identity> questionsToBeValid,
            out List<Identity> questionsToBeInvalid);

        void ProcessConditionExpressions(IInterviewExpressionState interviewExpressionState,
            out List<Identity> enabledGroups,
            out List<Identity> disabledGroups,
            out List<Identity> questionsToBeEnabled,
            out List<Identity> questionsToBeDisabled);
    }
}