using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WB.Core.GenericSubdomains.Portable
{
    public static class QueryableExtensions
    {
        public static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string fieldName)
            where TEntity : class
        {
            MethodCallExpression resultExp = GenerateMethodCall(source, "OrderBy", fieldName);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        public static IOrderedQueryable<TEntity> OrderByDescending<TEntity>(
            this IQueryable<TEntity> source, string fieldName) where TEntity : class
        {
            MethodCallExpression resultExp = GenerateMethodCall(source, "OrderByDescending", fieldName);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        public static IQueryable<TEntity> OrderUsingSortExpression<TEntity>(
            this IQueryable<TEntity> source, string sortExpression) where TEntity : class
        {
            IEnumerable<OrderRequestItem> orderFields = ParseSortExpression(sortExpression);

            if (orderFields == null)
                return source;

            IOrderedQueryable<TEntity> result = null;

            bool isFirstOrderField = true;
            foreach (var orderField in orderFields)
            {
                var sortField = orderField.Field;

                if (orderField.Direction == OrderDirection.Asc)
                {
                    result = isFirstOrderField ? source.OrderBy(sortField) : result.ThenBy(sortField);
                }
                else
                {
                    result = isFirstOrderField
                        ? source.OrderByDescending(sortField)
                        : result.ThenByDescending(sortField);
                }
                isFirstOrderField = false;
            }
            return result ?? source;
        }


        private static IEnumerable<OrderRequestItem> ParseSortExpression(string sortExpression)
        {
            string[] orderFields = sortExpression.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            if (!orderFields.Any())
                return null;

            var orders = new List<OrderRequestItem>();

            var delimiters = new[] {" "};

            for (int currentFieldIndex = 0; currentFieldIndex < orderFields.Count(); currentFieldIndex++)
            {
                string[] expressionPart =
                    orderFields[currentFieldIndex].Trim().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string sortField = expressionPart[0];
                bool sortDescending = (expressionPart.Length == 2)
                    && expressionPart[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);

                orders.Add(new OrderRequestItem(){Field = sortField, Direction = sortDescending ? OrderDirection.Desc : OrderDirection.Asc});
            }

            return orders;
        }

        public static IOrderedQueryable<TEntity> ThenBy<TEntity>(
            this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
        {
            MethodCallExpression resultExp = GenerateMethodCall(source, "ThenBy", fieldName);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        public static IOrderedQueryable<TEntity> ThenByDescending<TEntity>(
            this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
        {
            MethodCallExpression resultExp = GenerateMethodCall(source, "ThenByDescending", fieldName);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        private static MethodCallExpression GenerateMethodCall<TEntity>(
            IQueryable<TEntity> source, string methodName, string fieldName) where TEntity : class
        {
            Type type = typeof(TEntity);
            Type selectorResultType;
            LambdaExpression selector = GenerateSelector<TEntity>(fieldName, out selectorResultType);
            MethodCallExpression resultExp = Expression.Call(
                typeof(Queryable), 
                methodName, 
                new[] { type, selectorResultType }, 
                source.Expression, 
                Expression.Quote(selector));
            return resultExp;
        }

        private static LambdaExpression GenerateSelector<TEntity>(string propertyName, out Type resultType)
            where TEntity : class
        {
            // Create a parameter to pass into the Lambda expression (Entity => Entity.OrderByField).
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "Entity");

            // create the selector part, but support child properties
            PropertyInfo property;
            Expression propertyAccess;
            if (propertyName.Contains("."))
            {
                // support to be sorted on child fields.
                var childProperties = propertyName.Split('.');
                property = typeof(TEntity).GetRuntimeProperty(childProperties[0]);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
                
                for (var i = 1; i < childProperties.Length; i++)
                {
                    property = property.PropertyType.GetRuntimeProperty(childProperties[i]);
                    propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                }
            }
            else
            {
                property = typeof(TEntity).GetRuntimeProperty(propertyName);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
            }

            resultType = property.PropertyType;

            // Create the order by expression.
            return Expression.Lambda(propertyAccess, parameter);
        }
    }
}