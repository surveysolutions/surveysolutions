using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.ExpressionProcessor.Services
{
    /// <summary>
    /// Expression processor which incapsulates all operations with expressions such as analysis and evaluation.
    /// </summary>
    public interface IExpressionProcessor
    {
        [Obsolete("Now a separate engine verifies validity of C# expressions")]
        bool IsSyntaxValid(string expression);

        IEnumerable<string> GetIdentifiersUsedInExpression(string expression);

        [Obsolete("Now a separate engine evaluates C# expressions")]
        bool EvaluateBooleanExpression(string expression, Func<string, object> getValueForIdentifier);
    }
}
