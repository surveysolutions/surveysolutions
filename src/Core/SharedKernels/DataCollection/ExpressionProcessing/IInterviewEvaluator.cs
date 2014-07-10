using System.Collections.Generic;

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    public interface IInterviewEvaluator
    {
        int Test();

        List<Identity> CalculateValidationChanges();

        List<Identity> CalculateConditionChanges();
    }
}
