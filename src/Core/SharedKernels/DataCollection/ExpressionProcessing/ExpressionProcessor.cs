using System.Collections.Generic;

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    public class ExpressionProcessor : IExpressionProcessor
    {
        public void ProcessValidationExpressions(IInterviewExpressionState interviewExpressionState, out List<Identity> questionsToBeValid,
            out List<Identity> questionsToBeInvalid)
        {
            questionsToBeValid = new List<Identity>();
            questionsToBeInvalid = new List<Identity>();


            interviewExpressionState.ProcessValidationExpressions(questionsToBeValid, questionsToBeInvalid);
        }

        public void ProcessConditionExpressions(IInterviewExpressionState interviewExpressionState,
            out List<Identity> enabledGroups,
            out List<Identity> disabledGroups,
            out List<Identity> questionsToBeEnabled,
            out List<Identity> questionsToBeDisabled)
        {
            enabledGroups = new List<Identity>();
            disabledGroups = new List<Identity>();

            questionsToBeEnabled = new List<Identity>();
            questionsToBeDisabled = new List<Identity>();

            interviewExpressionState.ProcessConditionExpressions(questionsToBeEnabled, questionsToBeDisabled, enabledGroups, disabledGroups);
        }
    }
}