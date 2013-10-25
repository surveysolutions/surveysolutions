using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    /// <summary>
    /// Expression processor which incapsulates all operations with expressions such as analysis and evaluation.
    /// </summary>
    internal interface IExpressionProcessor
    {
        bool IsSyntaxValid(string expression);

        IEnumerable<string> GetIdentifiersUsedInExpression(string expression);
    }
}
