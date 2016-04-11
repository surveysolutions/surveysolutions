using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.V7;

namespace WB.Core.SharedKernels.DataCollection.V8
{
    public interface IExpressionExecutableV8 : IExpressionExecutableV7
    {
        EnablementChanges ProcessEnablementConditions();
    }
}