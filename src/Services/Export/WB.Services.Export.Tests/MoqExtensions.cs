using System;
using System.Linq.Expressions;
using Moq;
using Moq.Language.Flow;

namespace WB.Services.Export.Tests
{
    public static class MoqExtensions
    {
        public static ISetup<T, TResult> SetupIgnoreArgs<T, TResult>(this Mock<T> mock,
            Expression<Func<T, TResult>> expression)
            where T : class
        {
            expression = new MakeAnyVisitor().VisitAndConvert(
                expression, "SetupIgnoreArgs");

            return mock.Setup(expression);
        }

        public static ISetup<T> SetupIgnoreArgs<T>(this Mock<T> mock,
            Expression<Action<T>> expression)
            where T : class
        {
            expression = new MakeAnyVisitor().VisitAndConvert(
                expression, "SetupIgnoreArgs");

            return mock.Setup(expression);
        }

        public static void VerifyIgnoreArgs<T>(this Mock<T> mock,
            Expression<Action<T>> expression, Func<Times> times = null)
            where T : class
        {
            expression = new MakeAnyVisitor().VisitAndConvert(
                expression, "VerifyIgnoreArgs");

            mock.Verify(expression, times ?? Times.AtLeastOnce);
        }

        public static void VerifyIgnoreArgs<T, TResult>(this Mock<T> mock,
            Expression<Func<T, TResult>> expression, Func<Times> times = null)
            where T : class
        {
            expression = new MakeAnyVisitor().VisitAndConvert(
                expression, "VerifyIgnoreArgs");

            mock.Verify(expression, times ?? Times.AtLeastOnce);
        }

        private class MakeAnyVisitor : ExpressionVisitor
        {
            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Value != null)
                    return base.VisitConstant(node);

                var method = typeof(It)
                    .GetMethod("IsAny")
                    .MakeGenericMethod(node.Type);

                return Expression.Call(method);
            }
        }
    }
}
