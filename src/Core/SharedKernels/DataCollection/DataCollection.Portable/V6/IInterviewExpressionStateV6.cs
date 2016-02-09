using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V5;

namespace WB.Core.SharedKernels.DataCollection.V6
{
    public interface IInterviewExpressionStateV6: IInterviewExpressionStateV5
    {
        void ApplyFailedValidations(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions);

        new IInterviewExpressionStateV6 Clone();
    }
}