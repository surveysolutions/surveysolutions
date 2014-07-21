using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection
{
    public interface IInterviewEvaluator
    {
        int Test();

        List<Identity> CalculateValidationChanges();

        List<Identity> CalculateConditionChanges();
    }
}
