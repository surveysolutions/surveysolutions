using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.V8
{
    public interface IInterviewExpressionStateV8 : IInterviewExpressionStateV7
    {
        void DisableStaticTexts(IEnumerable<Identity> staticTextsToDisable);
        void EnableStaticTexts(IEnumerable<Identity> staticTextsToEnable);

        void DeclareStaticTextValid(IEnumerable<Identity> validStaticTexts);
        void ApplyStaticTextFailedValidations(IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions);

        new IInterviewExpressionStateV8 Clone();
    }
}