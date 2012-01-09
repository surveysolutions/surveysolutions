using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public interface IExpressionExecutor<TInput>
    {
        bool Execute(TInput entity, string condition);
    }
}
