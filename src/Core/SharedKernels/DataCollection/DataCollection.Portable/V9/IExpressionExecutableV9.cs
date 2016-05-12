using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V8;

namespace WB.Core.SharedKernels.DataCollection.V9
{
    public interface IExpressionExecutableV9 : IExpressionExecutableV8
    {
        IExpressionExecutableV9 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV9>> getInstances);
        void SetParent(IExpressionExecutableV9 parentLevel);

        void EnableVariable(Guid variableId);
        void DisableVariable(Guid variableId);
        void UpdateVariableValue(Guid variableId, object value);
        VariableValueChanges ProcessVariables();

        new IExpressionExecutableV9 GetParent();
        new IExpressionExecutableV9 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey);
    }
}