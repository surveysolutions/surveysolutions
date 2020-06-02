using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class MacrosSubstitutionService : IMacrosSubstitutionService
    {
        public string InlineMacros(string? expression, IEnumerable<Macro> macros)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return string.Empty;

            var expressionContainsMacrosMarker = expression.Contains("$");
            if (!expressionContainsMacrosMarker)
                return expression;

            var resultExpression = expression;
            foreach (var macro in macros.OrderByDescending(x => x.Name))
            {
                resultExpression = resultExpression.Replace("$" + macro.Name, macro.Content);
            }

            return resultExpression;
        }
    }
}
