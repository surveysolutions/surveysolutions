using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public class ExpressionProcessor : IExpressionProcessor
    {
        public void ProcessValidationExpressions(IInterviewExpressionState interviewExpressionState, out List<Identity> questionsToBeValid,
            out List<Identity> questionsToBeInvalid)
        {
            interviewExpressionState.ProcessValidationExpressions(out questionsToBeValid,out questionsToBeInvalid);
        }

        public void ProcessConditionExpressions(IInterviewExpressionState interviewExpressionState,
            out List<Identity> enabledGroups,
            out List<Identity> disabledGroups,
            out List<Identity> questionsToBeEnabled,
            out List<Identity> questionsToBeDisabled)
        {
            interviewExpressionState.ProcessConditionExpressions(out questionsToBeEnabled, out questionsToBeDisabled, out enabledGroups, out disabledGroups);
        }
    }
}