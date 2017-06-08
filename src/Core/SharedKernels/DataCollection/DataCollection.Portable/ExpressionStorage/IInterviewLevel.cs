using System;

namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage
{
    public interface IInterviewLevel
    {
        int RowCode { get; }
        int RowIndex { get; }

        Func<bool> GetConditionExpression(Identity entitIdentity);
        Func<bool>[] GetValidationExpressions(Identity entitIdentity);
        Func<IInterviewLevel, bool> GetLinkedQuestionFilter(Identity identity);
        Func<object> GetVariableExpression(Identity identity);
        Func<int, bool> GetCategoricalFilter(Identity identity);
    }
}