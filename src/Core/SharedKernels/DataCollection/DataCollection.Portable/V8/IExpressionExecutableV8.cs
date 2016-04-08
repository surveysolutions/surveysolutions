using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.V8
{
    public interface IExpressionExecutableV8 : IExpressionExecutableV7
    {
        void CalculateConditionChanges(
            out List<Identity> questionsToBeEnabled, out List<Identity> questionsToBeDisabled,
            out List<Identity> groupsToBeEnabled, out List<Identity> groupsToBeDisabled,
            out List<Identity> staticTextsToBeEnabled, out List<Identity> staticTextsToBeDisabled);
    }
}