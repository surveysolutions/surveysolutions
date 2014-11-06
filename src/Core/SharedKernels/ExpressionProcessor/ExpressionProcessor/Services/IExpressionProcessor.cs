using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.ExpressionProcessor.Services
{
    /// <summary>
    /// Expression processor which incapsulates all operations with expressions such as analysis and evaluation.
    /// </summary>
    public interface IExpressionProcessor
    {
        IEnumerable<string> GetIdentifiersUsedInExpression(string expression);
    }
}
