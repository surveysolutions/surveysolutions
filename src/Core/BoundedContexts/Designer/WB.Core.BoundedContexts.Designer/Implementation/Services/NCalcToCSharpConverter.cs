using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NCalc;
using NCalc.Domain;
using ValueType = NCalc.Domain.ValueType;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class NCalcToCSharpConverter
    {
        internal class CSharpSerializationVisitor : LogicalExpressionVisitor
        {
            private readonly Dictionary<string, string> customIdentifierMappings;
            private readonly StringBuilder builder = new StringBuilder();

            public CSharpSerializationVisitor(Dictionary<string, string> customIdentifierMappings)
            {
                this.customIdentifierMappings = customIdentifierMappings ?? new Dictionary<string, string>();
            }

            public string GetResultCSharpExpression()
            {
                return this.builder.ToString().Trim();
            }

            public override void Visit(LogicalExpression expression)
            {
                throw new NotImplementedException();
            }

            public override void Visit(TernaryExpression expression)
            {
                this.EncapsulateNoValue(expression.LeftExpression);

                this.builder.Append("? ");

                this.EncapsulateNoValue(expression.MiddleExpression);

                this.builder.Append(": ");

                this.EncapsulateNoValue(expression.RightExpression);
            }

            public override void Visit(BinaryExpression expression)
            {
                this.EncapsulateNoValue(expression.LeftExpression);

                switch (expression.Type)
                {
                    case BinaryExpressionType.And:
                        this.builder.Append("&& ");
                        break;

                    case BinaryExpressionType.Or:
                        this.builder.Append("|| ");
                        break;

                    case BinaryExpressionType.Div:
                        this.builder.Append("/ ");
                        break;

                    case BinaryExpressionType.Equal:
                        this.builder.Append("== ");
                        break;

                    case BinaryExpressionType.Greater:
                        this.builder.Append("> ");
                        break;

                    case BinaryExpressionType.GreaterOrEqual:
                        this.builder.Append(">= ");
                        break;

                    case BinaryExpressionType.Lesser:
                        this.builder.Append("< ");
                        break;

                    case BinaryExpressionType.LesserOrEqual:
                        this.builder.Append("<= ");
                        break;

                    case BinaryExpressionType.Minus:
                        this.builder.Append("- ");
                        break;

                    case BinaryExpressionType.Modulo:
                        this.builder.Append("% ");
                        break;

                    case BinaryExpressionType.NotEqual:
                        this.builder.Append("!= ");
                        break;

                    case BinaryExpressionType.Plus:
                        this.builder.Append("+ ");
                        break;

                    case BinaryExpressionType.Times:
                        this.builder.Append("* ");
                        break;

                    case BinaryExpressionType.BitwiseAnd:
                        this.builder.Append("& ");
                        break;

                    case BinaryExpressionType.BitwiseOr:
                        this.builder.Append("| ");
                        break;

                    case BinaryExpressionType.BitwiseXOr:
                        this.builder.Append("~ ");
                        break;

                    case BinaryExpressionType.LeftShift:
                        this.builder.Append("<< ");
                        break;

                    case BinaryExpressionType.RightShift:
                        this.builder.Append(">> ");
                        break;
                }

                this.EncapsulateNoValue(expression.RightExpression);
            }

            public override void Visit(UnaryExpression expression)
            {
                switch (expression.Type)
                {
                    case UnaryExpressionType.Not:
                        this.builder.Append("!");
                        break;

                    case UnaryExpressionType.Negate:
                        this.builder.Append("-");
                        break;

                    case UnaryExpressionType.BitwiseNot:
                        this.builder.Append("~");
                        break;
                }

                this.EncapsulateNoValue(expression.Expression);
            }

            public override void Visit(ValueExpression expression)
            {
                switch (expression.Type)
                {
                    case ValueType.Boolean:
                        this.builder.Append(expression.Value.ToString()).Append(" ");
                        break;

                    case ValueType.DateTime:
                        this.builder.Append("#").Append(expression.Value.ToString()).Append("#").Append(" ");
                        break;

                    case ValueType.Float:
                        this.builder.Append(decimal.Parse(expression.Value.ToString()).ToString(CultureInfo.InvariantCulture)).Append(" ");
                        break;

                    case ValueType.Integer:
                        this.builder.Append(expression.Value.ToString()).Append(" ");
                        break;

                    case ValueType.String:
                        this.builder.Append("'").Append(expression.Value.ToString()).Append("'").Append(" ");
                        break;
                }
            }

            public override void Visit(Function function)
            {
                bool isContainsFunction = function.Identifier.Name.Trim().ToLower() == "contains" && function.Expressions.Length == 2;

                if (isContainsFunction)
                {
                    function.Expressions[0].Accept(this);

                    TrimEndingSpaces(this.builder);

                    this.builder.Append(".Contains(");

                    function.Expressions[1].Accept(this);

                    TrimEndingSpaces(this.builder);

                    this.builder.Append(") ");
                }
                else
                {
                    this.builder.Append(function.Identifier.Name);

                    this.builder.Append("(");

                    for (int i = 0; i < function.Expressions.Length; i++)
                    {
                        function.Expressions[i].Accept(this);
                        if (i < function.Expressions.Length - 1)
                        {
                            this.builder.Remove(this.builder.Length - 1, 1);
                            this.builder.Append(", ");
                        }
                    }

                    TrimEndingSpaces(this.builder);

                    this.builder.Append(") ");
                }
            }

            private static void TrimEndingSpaces(StringBuilder stringBuilder)
            {
                while (stringBuilder[stringBuilder.Length - 1] == ' ')
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }

            public override void Visit(Identifier parameter)
            {
                string ncalcIdentifier = parameter.Name;

                string roslynIdentifier = customIdentifierMappings.ContainsKey(ncalcIdentifier)
                    ? customIdentifierMappings[ncalcIdentifier]
                    : ncalcIdentifier;

                this.builder.Append(roslynIdentifier).Append(" ");
            }

            private void EncapsulateNoValue(LogicalExpression expression)
            {
                if (expression is ValueExpression || expression is Identifier)
                {
                    expression.Accept(this);
                }
                else
                {
                    this.builder.Append("(");
                    expression.Accept(this);

                    // trim spaces before adding a closing paren
                    while (this.builder[this.builder.Length - 1] == ' ')
                        this.builder.Remove(this.builder.Length - 1, 1);

                    this.builder.Append(") ");
                }
            }
        }

        public string Convert(string ncalcExpression, Dictionary<string, string> customMappings)
        {
            LogicalExpression ncalcExpressionTree = ParseExpressionOrThrow(ncalcExpression);

            var roslynSerializationVisitor = new CSharpSerializationVisitor(customMappings);

            ncalcExpressionTree.Accept(roslynSerializationVisitor);

            string roslynExpression = roslynSerializationVisitor.GetResultCSharpExpression();

            return roslynExpression;
        }

        private static LogicalExpression ParseExpressionOrThrow(string stringExpression)
        {
            var expression = new Expression(stringExpression);

            if (expression.HasErrors())
                throw new ArgumentException(string.Format("NCalc expression {0} has errors", stringExpression));

            return expression.ParsedExpression;
        }
    }
}