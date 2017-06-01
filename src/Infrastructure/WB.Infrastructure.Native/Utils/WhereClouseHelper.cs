using System;
using System.Linq.Expressions;

namespace WB.Infrastructure.Native.Utils
{
    public static class WhereClouseHelper
    {
        public static Expression<Func<T, bool>> AndCondition<T>(this Expression<Func<T, bool>> predicate, Expression<Func<T, bool>> condition)
        {
            
            if (predicate.Body.NodeType==ExpressionType.Constant)
            {
                return condition;
            }

            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(predicate.Body, condition.Body), predicate.Parameters);
        }

        public static Expression<Func<T, bool>> OrCondition<T>(this Expression<Func<T, bool>> predicate, Expression<Func<T, bool>> condition)
        {

            if (predicate.Body.NodeType == ExpressionType.Constant)
            {
                return condition;
            }

            return Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(predicate.Body, condition.Body), predicate.Parameters);
        }
    }
}