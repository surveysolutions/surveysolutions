using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class MacrosSubstitutionService : IMacrosSubstitutionService
    {
        public string SubstituteMacroses(string expression, QuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return expression;

            var expressionContainsMacrosMarker = expression.Contains("$");
            if (!expressionContainsMacrosMarker)
                return expression;

            var resultExpression = expression;
            foreach (var macros in questionnaire.Macroses.Values)
            {
                resultExpression = resultExpression.Replace("$" + macros.Name, macros.Expression);
            }

            return resultExpression;
        }
    }
}
