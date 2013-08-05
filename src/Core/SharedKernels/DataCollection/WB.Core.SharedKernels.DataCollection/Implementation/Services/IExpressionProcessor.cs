using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    internal interface IExpressionProcessor
    {
        IEnumerable<string> GetIdentifiersUsedInExpression(string expression);
    }
}
