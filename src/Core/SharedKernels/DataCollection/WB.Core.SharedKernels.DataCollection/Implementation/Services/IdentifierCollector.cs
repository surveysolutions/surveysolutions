using System.Collections.Generic;
using NCalc.Domain;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Services
{
    internal class IdentifierCollector : LogicalExpressionVisitor
    {
        private readonly HashSet<string> identifiers = new HashSet<string>();

        public override void Visit(LogicalExpression expression) { }

        public override void Visit(TernaryExpression expression)
        {
            expression.LeftExpression.Accept(this);
            expression.MiddleExpression.Accept(this);
            expression.RightExpression.Accept(this);
        }

        public override void Visit(BinaryExpression expression)
        {
            expression.LeftExpression.Accept(this);
            expression.RightExpression.Accept(this);
        }

        public override void Visit(UnaryExpression expression)
        {
            expression.Expression.Accept(this);
        }

        public override void Visit(ValueExpression expression) { }

        public override void Visit(Function function)
        {
            foreach (LogicalExpression expression in function.Expressions)
            {
                expression.Accept(this);
            }
        }

        public override void Visit(Identifier identifier)
        {
            this.identifiers.Add(identifier.Name);
        }

        public IEnumerable<string> GetCollectedIdentifiers()
        {
            return this.identifiers;
        }
    }
}