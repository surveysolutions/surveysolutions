using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using NCalc.Domain;
using WB.Core.SharedKernels.ExpressionProcessor.Services;

namespace WB.Core.SharedKernels.ExpressionProcessor.Implementation.Services
{
    internal class ExpressionProcessor : IExpressionProcessor
    {
        public bool IsSyntaxValid(string expression)
        {
            var ncalcExpression = new Expression(expression);

            return !ncalcExpression.HasErrors();
        }

        public IEnumerable<string> GetIdentifiersUsedInExpression(string expression)
        {
            LogicalExpression parsedExpression = ParseExpressionOrThrow(expression);

            if (parsedExpression == null)
                return Enumerable.Empty<string>();

            var identifierCollector = new IdentifierCollector();

            parsedExpression.Accept(identifierCollector);

            return identifierCollector.GetCollectedIdentifiers();
        }

        public bool EvaluateBooleanExpression(string expression, Func<string, object> getValueForIdentifier)
        {
            var evaluatableExpression = new Expression(expression);

            AddSupportForMultipleOptionsAnswerFunction(evaluatableExpression);

            evaluatableExpression.EvaluateParameter += (name, args) =>
            {
                args.Result = getValueForIdentifier(name);
            };

            return (bool)evaluatableExpression.Evaluate();
        }

        private static void AddSupportForMultipleOptionsAnswerFunction(Expression evaluatableExpression)
        {
            evaluatableExpression.EvaluateFunction += ExtensionFunctions.EvaluateFunctionContains;
        }

        private static LogicalExpression ParseExpressionOrThrow(string stringExpression)
        {
            var expression = new Expression(stringExpression);

            bool hasErrors = expression.HasErrors(); // HasErrors method forces expression to be parsed (initializes ParsedExpression property)

            return hasErrors
                ? null
                : expression.ParsedExpression;
        }
    }
}