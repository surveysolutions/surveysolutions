using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class Reflect<T>
    {
        public static string MethodName(Expression<Func<T, Delegate>> expression)
        {
            return GetNameFromExpression((UnaryExpression)expression.Body);
        }

        public static string MethodName(LambdaExpression expression)
        {
            return GetNameFromExpression((UnaryExpression)expression.Body);
        }

        private static string GetNameFromExpression(UnaryExpression unaryExpression)
        {
            var methodCallExpression = (MethodCallExpression) unaryExpression.Operand;
            var methodCallObject = (ConstantExpression) methodCallExpression.Object;
            var methodInfo = (MethodInfo) methodCallObject.Value;
            return methodInfo.Name;
        }

        public static string ShortPropertyName<TPropertyType>(Expression<Func<T, TPropertyType>> exp)
        {
            return PropertyNameFromExpresion(exp.Body, fullPath: false);
        }

        public static string PropertyName<TPropertyType>(Expression<Func<T, TPropertyType>> exp)
        {
            return PropertyNameFromExpresion(exp.Body);
        }

        private static string PropertyNameFromExpresion(Expression exp, bool fullPath = true)
        {
            MemberExpression memberExpression = exp as MemberExpression;
            if (memberExpression == null && exp is UnaryExpression)
            {
                UnaryExpression unaryExpression = (UnaryExpression) exp;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }

            if (memberExpression == null)
                return "";

            var name = memberExpression.Member.Name;

            if (fullPath)
            {
                string prefix = PropertyNameFromExpresion(memberExpression.Expression);

                if (!prefix.IsNullOrEmpty())
                    return prefix + "." + name;
            }

            return name;
        }
    }
}