using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal interface IRoslynExpressionAnalyser
    {
        IEnumerable<string> ExtractVariables(string expression);
    }
}