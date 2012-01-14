using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public interface IExpressionExecutor<in TInput, out TOutput>
    {
        TOutput Execute(TInput entity, string condition);
    }
}
