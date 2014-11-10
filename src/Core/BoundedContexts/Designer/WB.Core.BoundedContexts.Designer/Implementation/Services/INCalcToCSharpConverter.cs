using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal interface INCalcToCSharpConverter
    {
        string Convert(string ncalcExpression, Dictionary<string, string> customMappings);
    }
}