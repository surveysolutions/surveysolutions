using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        public static void AddMockObject<TService, TResult>(this ServiceCollection services,
            Expression<Func<TService, TResult>> setup, TResult returns) where TService : class
        {
            var mock = new Mock<TService>();

            mock.Setup(setup).Returns(returns);

            services.AddTransient(s => mock.Object);
        }

        public static void AddMock<TService>(this ServiceCollection coll, Expression<Func<TService, bool>> predicate = null) where TService : class
        {
            coll.AddTransient(c => predicate == null ? Mock.Of<TService>() : Mock.Of(predicate));
        }

        public static void AddScopedMock<TService>(this ServiceCollection coll, Expression<Func<TService, bool>> predicate = null) where TService : class
        {
            coll.AddScoped(c => predicate == null ? Mock.Of<TService>() : Mock.Of(predicate));
        }

        public static void Verify<T>(this Mock<ILogger<T>> logMock, LogLevel level, Predicate<Exception> exception, Func<Times> times)
        {            
            logMock.Verify(l => l.Log(level, 0, It.IsAny<It.IsAnyType>(), 
                Match.Create(exception), (Func<It.IsAnyType, Exception, string>) It.IsAny<object>()), times);
        }
    }
}
