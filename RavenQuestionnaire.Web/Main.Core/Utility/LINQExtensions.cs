namespace Main.Core.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    ///     The string field name sorting support.
    /// </summary>
    public static class StringFieldNameSortingSupport
    {
        #region Public Methods and Operators

        /// <summary>
        /// The order by.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="fieldName">
        /// The field name.
        /// </param>
        /// <typeparam name="TEntity">
        /// </typeparam>
        /// <returns>
        /// The System.Linq.IOrderedQueryable`1[T -&gt; TEntity].
        /// </returns>
        public static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string fieldName)
            where TEntity : class
        {
            MethodCallExpression resultExp = GenerateMethodCall(source, "OrderBy", fieldName);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        /// <summary>
        /// The order by descending.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="fieldName">
        /// The field name.
        /// </param>
        /// <typeparam name="TEntity">
        /// </typeparam>
        /// <returns>
        /// The System.Linq.IOrderedQueryable`1[T -&gt; TEntity].
        /// </returns>
        public static IOrderedQueryable<TEntity> OrderByDescending<TEntity>(
            this IQueryable<TEntity> source, string fieldName) where TEntity : class
        {
            MethodCallExpression resultExp = GenerateMethodCall(source, "OrderByDescending", fieldName);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        /// <summary>
        /// The order using sort expression.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="sortExpression">
        /// The sort expression.
        /// </param>
        /// <typeparam name="TEntity">
        /// </typeparam>
        /// <returns>
        /// The System.Linq.IOrderedQueryable`1[T -&gt; TEntity].
        /// </returns>
        public static IEnumerable<TEntity> OrderUsingSortExpression<TEntity>(
            this IQueryable<TEntity> source, string sortExpression) where TEntity : class
        {
            string[] orderFields = sortExpression.Split(',').Where(x => !string.IsNullOrEmpty(x)).ToArray();
            IOrderedQueryable<TEntity> result = null;
            var delimiters = new[] { " " };
            for (int currentFieldIndex = 0; currentFieldIndex < orderFields.Count(); currentFieldIndex++)
            {
                string[] expressionPart =
                    orderFields[currentFieldIndex].Trim().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                string sortField = expressionPart[0];
                bool sortDescending = (expressionPart.Length == 2)
                                      && expressionPart[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);
                if (sortDescending)
                {
                    result = currentFieldIndex == 0
                                 ? source.OrderByDescending(sortField)
                                 : result.ThenByDescending(sortField);
                }
                else
                {
                    result = currentFieldIndex == 0 ? source.OrderBy(sortField) : result.ThenBy(sortField);
                }
            }

            return result ?? source;
        }

        /// <summary>
        /// The then by.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="fieldName">
        /// The field name.
        /// </param>
        /// <typeparam name="TEntity">
        /// </typeparam>
        /// <returns>
        /// The System.Linq.IOrderedQueryable`1[T -&gt; TEntity].
        /// </returns>
        public static IOrderedQueryable<TEntity> ThenBy<TEntity>(
            this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
        {
            MethodCallExpression resultExp = GenerateMethodCall(source, "ThenBy", fieldName);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        /// <summary>
        /// The then by descending.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="fieldName">
        /// The field name.
        /// </param>
        /// <typeparam name="TEntity">
        /// </typeparam>
        /// <returns>
        /// The System.Linq.IOrderedQueryable`1[T -&gt; TEntity].
        /// </returns>
        public static IOrderedQueryable<TEntity> ThenByDescending<TEntity>(
            this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
        {
            MethodCallExpression resultExp = GenerateMethodCall(source, "ThenByDescending", fieldName);
            return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The generate method call.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="methodName">
        /// The method name.
        /// </param>
        /// <param name="fieldName">
        /// The field name.
        /// </param>
        /// <typeparam name="TEntity">
        /// </typeparam>
        /// <returns>
        /// The System.Linq.Expressions.MethodCallExpression.
        /// </returns>
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

        /// <summary>
        /// The generate selector.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <param name="resultType">
        /// The result type.
        /// </param>
        /// <typeparam name="TEntity">
        /// </typeparam>
        /// <returns>
        /// The System.Linq.Expressions.LambdaExpression.
        /// </returns>
        private static LambdaExpression GenerateSelector<TEntity>(string propertyName, out Type resultType)
            where TEntity : class
        {
            // Create a parameter to pass into the Lambda expression (Entity => Entity.OrderByField).
            ParameterExpression parameter = Expression.Parameter(typeof(TEntity), "Entity");

            // create the selector part, but support child properties
            PropertyInfo property;
            Expression propertyAccess;
            if (propertyName.Contains('.'))
            {
                // support to be sorted on child fields.
                string[] childProperties = propertyName.Split('.');
                property = typeof(TEntity).GetProperty(
                    childProperties[0], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
                for (int i = 1; i < childProperties.Length; i++)
                {
                    property = property.PropertyType.GetProperty(
                        childProperties[i], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
                }
            }
            else
            {
                property = typeof(TEntity).GetProperty(
                    propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                propertyAccess = Expression.MakeMemberAccess(parameter, property);
            }

            resultType = property.PropertyType;

            // Create the order by expression.
            return Expression.Lambda(propertyAccess, parameter);
        }

        #endregion
    }

    /// <summary>
    /// The linq helper.
    /// </summary>
    public static class LinqHelper
    {
        #region Public Methods and Operators

        /// <summary>
        /// The descendants.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="descendBy">
        /// The descend by.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="rootElement">
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        public static void ApplyAction<T>(
            this IEnumerable<T> source,
            Func<T, IEnumerable<T>> descendBy,
            Action<T, T> action,
            T rootElement = default(T))
        {
            if (action == null)
            {
                return;
            }

            foreach (T value in source)
            {
                action(rootElement, value);

                descendBy(value).ApplyAction(descendBy, action, value);
            }
        }

        /// <summary>
        /// The and also.
        /// </summary>
        /// <param name="predicate1">
        /// The predicate 1.
        /// </param>
        /// <param name="predicate2">
        /// The predicate 2.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Func{T,TResult}"/>.
        /// </returns>
        public static Func<T, bool> AndAlso<T>(this Func<T, bool> predicate1, Func<T, bool> predicate2)
        {
            return arg => predicate1(arg) && predicate2(arg);
        }

        /// <summary>
        /// The or else.
        /// </summary>
        /// <param name="predicate1">
        /// The predicate 1.
        /// </param>
        /// <param name="predicate2">
        /// The predicate 2.
        /// </param>
        /// <typeparam name="T">
        /// </typeparam>
        /// <returns>
        /// The <see cref="Func{T,TResult}"/>.
        /// </returns>
        public static Func<T, bool> OrElse<T>(this Func<T, bool> predicate1, Func<T, bool> predicate2)
        {
            return arg => predicate1(arg) || predicate2(arg);
        }

        #endregion
    }
}