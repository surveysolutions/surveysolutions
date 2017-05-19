using System;

namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage
{
    public interface IInterviewLevel
    {
        Func<bool> GetConditionExpression(Identity entitIdentity);
        Func<bool>[] GetValidationExpressions(Identity entitIdentity);
    }
}