using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RavenQuestionnaire.Core.ExpressionExecutors
{
    public interface IExpressionExecutor<TInput>
    {
        TInput Entity { get; }
        bool Execute(string condition);
    }
}
