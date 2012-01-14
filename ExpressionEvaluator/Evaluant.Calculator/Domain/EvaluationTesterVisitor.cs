using System;
using System.Collections.Generic;
using System.Text;

namespace NCalc.Domain
{
    public class EvaluationTesterVisitor:EvaluationVisitor
    {
        public EvaluationTesterVisitor(EvaluateOptions options) : base(options)
        {
        }
        public override void Visit(BinaryExpression expression)
        {
            // simulate Lazy<Func<>> behavior for late evaluation
            object leftValue = null;
            Func<object> left = () =>
            {
                if (leftValue == null)
                {
                    expression.LeftExpression.Accept(this);
                    leftValue = Result;
                }
                return leftValue;
            };

            // simulate Lazy<Func<>> behavior for late evaluation
            object rightValue = null;
            Func<object> right = () =>
            {
                if (rightValue == null)
                {
                    expression.RightExpression.Accept(this);
                    rightValue = Result;
                }
                return rightValue;
            };

            switch (expression.Type)
            {
                case BinaryExpressionType.And:
                    Result = Convert.ToBoolean(left()) & Convert.ToBoolean(right());
                    break;

                case BinaryExpressionType.Or:
                    Result = Convert.ToBoolean(left()) | Convert.ToBoolean(right());
                    break;

                case BinaryExpressionType.Div:
                    Result = IsReal(left()) || IsReal(right())
                                 ? Numbers.Divide(left(), right())
                                 : Numbers.Divide(Convert.ToDouble(left()), right());
                    break;

                case BinaryExpressionType.Equal:
                    // Use the type of the left operand to make the comparison
                    Result = CompareUsingMostPreciseType(left(), right()) == 0;
                    break;

                case BinaryExpressionType.Greater:
                    // Use the type of the left operand to make the comparison
                    Result = CompareUsingMostPreciseType(left(), right()) > 0;
                    break;

                case BinaryExpressionType.GreaterOrEqual:
                    // Use the type of the left operand to make the comparison
                    Result = CompareUsingMostPreciseType(left(), right()) >= 0;
                    break;

                case BinaryExpressionType.Lesser:
                    // Use the type of the left operand to make the comparison
                    Result = CompareUsingMostPreciseType(left(), right()) < 0;
                    break;

                case BinaryExpressionType.LesserOrEqual:
                    // Use the type of the left operand to make the comparison
                    Result = CompareUsingMostPreciseType(left(), right()) <= 0;
                    break;

                case BinaryExpressionType.Minus:
                    Result = Numbers.Soustract(left(), right());
                    break;

                case BinaryExpressionType.Modulo:
                    Result = Numbers.Modulo(left(), right());
                    break;

                case BinaryExpressionType.NotEqual:
                    // Use the type of the left operand to make the comparison
                    Result = CompareUsingMostPreciseType(left(), right()) != 0;
                    break;

                case BinaryExpressionType.Plus:
                    if (left() is string)
                    {
                        Result = String.Concat(left(), right());
                    }
                    else
                    {
                        Result = Numbers.Add(left(), right());
                    }

                    break;

                case BinaryExpressionType.Times:
                    Result = Numbers.Multiply(left(), right());
                    break;

                case BinaryExpressionType.BitwiseAnd:
                    Result = Convert.ToUInt16(left()) & Convert.ToUInt16(right());
                    break;


                case BinaryExpressionType.BitwiseOr:
                    Result = Convert.ToUInt16(left()) | Convert.ToUInt16(right());
                    break;


                case BinaryExpressionType.BitwiseXOr:
                    Result = Convert.ToUInt16(left()) ^ Convert.ToUInt16(right());
                    break;


                case BinaryExpressionType.LeftShift:
                    Result = Convert.ToUInt16(left()) << Convert.ToUInt16(right());
                    break;


                case BinaryExpressionType.RightShift:
                    Result = Convert.ToUInt16(left()) >> Convert.ToUInt16(right());
                    break;
            }
        }
    }
}
