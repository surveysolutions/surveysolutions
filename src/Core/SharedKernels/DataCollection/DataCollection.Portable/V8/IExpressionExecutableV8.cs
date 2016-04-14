using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.V8
{
    public interface IExpressionExecutableV8 : IExpressionExecutableV7
    {
        IExpressionExecutableV8 CopyMembers(Func<Identity[], Guid, IEnumerable<IExpressionExecutableV8>> getInstances);
        void SetParent(IExpressionExecutableV8 parentLevel);
        new IExpressionExecutableV8 GetParent();
        new IExpressionExecutableV8 CreateChildRosterInstance(Guid rosterId, decimal[] rosterVector, Identity[] rosterIdentityKey);

        void DisableStaticText(Guid staticTextId);
        void EnableStaticText(Guid staticTextId);

        void ApplyStaticTextFailedValidations(Guid staticTextId, IReadOnlyList<FailedValidationCondition> failedValidations);
        void DeclareStaticTextValid(Guid staticTextId);

        EnablementChanges ProcessEnablementConditions();
    }
}