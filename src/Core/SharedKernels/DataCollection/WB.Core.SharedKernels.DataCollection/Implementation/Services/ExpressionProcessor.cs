﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.ExpressionExecutors.ExpressionExtentions;
using NCalc;
using NCalc.Domain;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    internal class ExpressionProcessor : IExpressionProcessor
    {
        public IEnumerable<string> GetIdentifiersUsedInExpression(string expression)
        {
            LogicalExpression parsedExpression = ParseExpressionOrReturnNull(expression);

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

            return (bool) evaluatableExpression.Evaluate();
        }

        private static void AddSupportForMultipleOptionsAnswerFunction(Expression evaluatableExpression)
        {
            evaluatableExpression.EvaluateFunction += ExtensionFunctions.EvaluateFunctionContains;
        }

        #warning TLK: this implementation is old and should not be used in verification kernel
        private static LogicalExpression ParseExpressionOrReturnNull(string stringExpression)
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