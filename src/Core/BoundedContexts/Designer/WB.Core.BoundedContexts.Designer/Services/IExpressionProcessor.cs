using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Services
{
    /// <summary>
    /// Expression processor which incapsulates all operations with expressions such as analysis and evaluation.
    /// </summary>
    public interface IExpressionProcessor
    {
        IReadOnlyCollection<string> GetIdentifiersUsedInExpression(string expression);
        bool ContainsBitwiseAnd(string expression);
    }
}
