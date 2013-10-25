using System;
using System.Collections.Generic;
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
            LogicalExpression parsedExpression = ParseExpressionOrThrow(expression);

            var identifierCollector = new IdentifierCollector();

            parsedExpression.Accept(identifierCollector);

            return identifierCollector.GetCollectedIdentifiers();
        }

        private static LogicalExpression ParseExpressionOrThrow(string stringExpression)
        {
            var expression = new Expression(stringExpression);

            bool hasErrors = expression.HasErrors(); // HasErrors method forces expression to be parsed (initializes ParsedExpression property)

            #warning TLK: decide do we need to throw exceptions in such cases or log it as warning and ignore
            if (hasErrors)
                throw new ArgumentException(string.Format("Failed to parse following expression: '{0}'.", stringExpression));

            return expression.ParsedExpression;
        }
    }
}