using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class Monads
    {
        /// <summary>
        /// Returns the value of an expression, or <c>default(T)</c> if any parts of the expression are <c>null</c>.
        /// </summary>
        /// <typeparam name="T">The type of the Expression</typeparam>
        /// <param name="expression">A parameterless lambda representing the path to the value.</param>
        /// <returns>The value of the expression, or <c>default(T)</c> if any parts of the expression are <c>null</c>.</returns>
        public static T Maybe<T>(Expression<Func<T>> expression)
        {
            var value = Maybe(expression.Body);
            if (value == null) return default(T);
            return (T)value;
        }

        private static object Maybe(Expression expression)
        {
            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null)
            {
                return constantExpression.Value;
            }

            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
            {
                var memberValue = Maybe(memberExpression.Expression);
                if (memberValue != null)
                {
                    var member = memberExpression.Member;
                    return GetValue(member, memberValue);
                }
            }

            var methodCallExpression = expression as MethodCallExpression;
            if (methodCallExpression != null)
            {
                var methodValue = Maybe(methodCallExpression.Object);
                var arguments = methodCallExpression.Arguments.Select(Maybe).ToArray();

                return GetValue(methodCallExpression.Method, methodValue, arguments);
            }

            var unaryExpression = expression as UnaryExpression;
            if (unaryExpression != null && unaryExpression.NodeType == ExpressionType.Convert)
            {
                var result = Maybe(unaryExpression.Operand);

                if (result != null && IsNullableType(unaryExpression.Type))
                {
                    return Activator.CreateInstance(unaryExpression.Type, result);
                }

                return result;
            }

            var binaryExpression = expression as BinaryExpression;
            if (binaryExpression != null && binaryExpression.NodeType == ExpressionType.Coalesce)
            {
                return Maybe(binaryExpression.Left) ?? Maybe(binaryExpression.Right);
            }

            return null;
        }

        private static object GetValue(MemberInfo member, object memberValue, object[] arguments = null)
        {
            var propertyInfo = member as PropertyInfo;
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(memberValue, null);
            }

            var fieldInfo = member as FieldInfo;
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(memberValue);
            }

            var methodInfo = member as MethodInfo;
            if (methodInfo != null && (memberValue != null || methodInfo.IsStatic))
            {
                return methodInfo.Invoke(memberValue, arguments);
            }

            return null;
        }

        private static bool IsNullableType(Type theType)
        {
            if (theType.GetTypeInfo().IsGenericType)
                return theType.GetGenericTypeDefinition() == typeof(Nullable<>);
            else
                return false;
        }
    }
}