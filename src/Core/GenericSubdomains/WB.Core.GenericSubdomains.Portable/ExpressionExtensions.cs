using System;
using System.Linq.Expressions;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class ExpressionExtensions
    {
        public static string GetName<TSource, TField>(Expression<Func<TSource, TField>> Field)
        {
            return (Field.Body as MemberExpression ?? ((UnaryExpression)Field.Body).Operand as MemberExpression).Member.Name;
        }
    }
}