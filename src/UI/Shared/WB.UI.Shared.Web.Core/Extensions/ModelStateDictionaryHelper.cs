using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WB.UI.Shared.Web.Extensions
{
    /// <summary>
    /// http://stackoverflow.com/questions/13564269/creating-a-typed-modelstate-addmodelerror
    /// </summary>
    public static class ModelStateDictionaryHelper
    {
        public static void AddModelError<TViewModel>(
            this ModelStateDictionary me,
            Expression<Func<TViewModel, object>> lambdaExpression, string error)
        {
            var propertyName = GetPropertyName(lambdaExpression);
            ArgumentNullException.ThrowIfNull(propertyName);
            me.AddModelError(propertyName, error);
        }

        private static string? GetPropertyName(Expression lambdaExpression)
        {
            IList<string?> list = new List<string?>();
            var e = lambdaExpression;

            while (true)
            {
                switch (e.NodeType)
                {
                    case ExpressionType.Lambda:
                        e = ((LambdaExpression)e).Body;
                        break;
                    case ExpressionType.MemberAccess:
                        var propertyInfo = ((MemberExpression)e).Member as PropertyInfo;
                        var prop = propertyInfo != null
                                          ? propertyInfo.Name
                                          : null;
                        list.Add(prop);

                        var memberExpression = (MemberExpression)e;
                        if (memberExpression.Expression != null && memberExpression.Expression.NodeType != ExpressionType.Parameter)
                        {
                            var parameter = GetParameterExpression(memberExpression.Expression);
                            if (parameter != null)
                            {
                                e = Expression.Lambda(memberExpression.Expression, parameter);
                                break;
                            }
                        }
                        return string.Join(".", list.Reverse());
                    default:
                        return null;
                }
            }
        }

        private static ParameterExpression? GetParameterExpression(Expression? expression)
        {
            while (expression != null && expression.NodeType == ExpressionType.MemberAccess)
            {
                expression = (expression as MemberExpression)?.Expression;
            }
            return expression?.NodeType == ExpressionType.Parameter ? (ParameterExpression)expression : null;
        }
    }
}
