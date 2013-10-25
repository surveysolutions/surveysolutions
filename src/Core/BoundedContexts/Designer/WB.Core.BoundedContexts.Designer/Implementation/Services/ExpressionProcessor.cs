using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.ExpressionExecutors.ExpressionExtentions;
using NCalc;
using NCalc.Domain;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
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
            LogicalExpression parsedExpression = ParseExpressionOrReturnNull(expression);

            if (parsedExpression == null)
                return Enumerable.Empty<string>();

            var identifierCollector = new IdentifierCollector();

            parsedExpression.Accept(identifierCollector);

            return identifierCollector.GetCollectedIdentifiers();
        }

        private static LogicalExpression ParseExpressionOrReturnNull(string stringExpression)
        {
            var expression = new Expression(stringExpression);

            bool hasErrors = expression.HasErrors(); // HasErrors method forces expression to be parsed (initializes ParsedExpression property)

            return hasErrors
                ? null
                : expression.ParsedExpression;
        }
    }
}