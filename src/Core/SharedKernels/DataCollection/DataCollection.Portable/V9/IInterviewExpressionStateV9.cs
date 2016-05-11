using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V8;

namespace WB.Core.SharedKernels.DataCollection.V9
{
    public interface IInterviewExpressionStateV9 : IInterviewExpressionStateV8
    {
        VariableValueChanges ProcessVariables();
        void DisableVariables(IEnumerable<Identity> variablesToDisable);
        void EnableVariables(IEnumerable<Identity> variablesToEnable);
        void SetPreviousVariableValue(Identity variableIdentity, object value);

        new IInterviewExpressionStateV9 Clone();
    }
}