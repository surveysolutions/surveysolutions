using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using NCalc.Domain;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    internal class ExpressionProcessor : IExpressionProcessor
    {
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