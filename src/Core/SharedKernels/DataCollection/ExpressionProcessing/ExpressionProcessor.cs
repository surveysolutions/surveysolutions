using System.Collections.Generic;
using WB.Core.Infrastructure.BaseStructures;

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
            List<Identity> enabledGroups,
            List<Identity> disabledGroups,
            List<Identity> enabledQuestions,
            List<Identity> disabledQuestions)
        {
            throw new System.NotImplementedException();
        }
    }
}