using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.V8
{
    public interface IInterviewExpressionStateV8 : IInterviewExpressionStateV7
    {
        void DisableStaticTexts(IEnumerable<Identity> staticTextsToDisable);
        void EnableStaticTexts(IEnumerable<Identity> staticTextsToEnable);
        new IInterviewExpressionStateV8 Clone();
    }
}