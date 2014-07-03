using System.Collections.Generic;

namespace WB.Core.Infrastructure.BaseStructures
{
    public interface IInterviewEvaluator
    {
        int Test();

        List<Identity> CalculateValidationChanges();

        List<Identity> CalculateConditionChanges();
    }
}
