using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    /// <summary>
    /// Expression processor which incapsulates all operations with expressions such as analysis and evaluation.
    /// </summary>
    internal interface IExpressionProcessor
    {
        IEnumerable<string> GetIdentifiersUsedInExpression(string expression);

        bool EvaluateBooleanExpression(string expression, Func<string, object> getValueForIdentifier);
    }
}
