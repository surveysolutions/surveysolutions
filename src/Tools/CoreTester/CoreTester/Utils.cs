using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace CoreTester
{
    public static class Utils
    {
        public static bool IsExistsMacrosesInDocument(QuestionnaireDocument questionnaireDocument)
        {
            bool isExistsMacros = false;

            var entities = questionnaireDocument.Children.TreeToEnumerable(x => x.Children);

            foreach (var entity in entities)
            {
                if (entity is IConditional conditionalEntity)
                {
                    isExistsMacros |= IsExpressionContainsMacros(conditionalEntity.ConditionExpression);
                }

                if (entity is IValidatable validatable)
                {
                    foreach (var validationCondition in validatable.ValidationConditions)
                    {
                        isExistsMacros |= IsExpressionContainsMacros(validationCondition.Expression);
                    }
                }

                if (entity is IQuestion question)
                {
                    isExistsMacros |= IsExpressionContainsMacros(question.Properties.OptionsFilterExpression);
                    isExistsMacros |= IsExpressionContainsMacros(question.LinkedFilterExpression);
                }

                if (entity is IVariable variable)
                {
                    isExistsMacros |= IsExpressionContainsMacros(variable.Expression);
                }

                if (isExistsMacros)
                    return true;
            }

            return isExistsMacros;
        }

        private static bool IsExpressionContainsMacros(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return false;

            var isExpressionContainsMacros = expression.Contains("$");
            //            if (isExpressionContainsMacros)
            //                Console.WriteLine("Found macros in condition: " + expression);

            return isExpressionContainsMacros;
        }
    }
}
