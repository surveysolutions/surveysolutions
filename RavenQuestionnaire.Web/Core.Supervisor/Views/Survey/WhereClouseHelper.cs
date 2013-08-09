using System;
using System.Linq.Expressions;
using Core.Supervisor.DenormalizerStorageItem;

namespace Core.Supervisor.Views.Survey
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
    }
}