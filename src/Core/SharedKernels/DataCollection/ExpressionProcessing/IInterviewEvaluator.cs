using System.Collections.Generic;
using WB.Core.Infrastructure.BaseStructures;

namespace WB.Core.SharedKernels.ExpressionProcessing
{
    public interface IInterviewEvaluator
    {
        int Test();

        List<Identity> CalculateValidationChanges();

        List<Identity> CalculateConditionChanges();
    }
}
