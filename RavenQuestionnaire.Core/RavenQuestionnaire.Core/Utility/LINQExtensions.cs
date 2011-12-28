/***** BEGIN LICENSE BLOCK *****
 * Version: MPL 1.1/GPL 2.0/LGPL 2.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is LINQExtensions.StringFieldNameSortingSupport.
 *
 * The Initial Developer of the Original Code is
 * Davy Landman.
 * Portions created by the Initial Developer are Copyright (C) 2008
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 *
 *
 * Alternatively, the contents of this file may be used under the terms of
 * either the GNU General Public License Version 2 or later (the "GPL"), or
 * the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
 * in which case the provisions of the GPL or the LGPL are applicable instead
 * of those above. If you wish to allow use of your version of this file only
 * under the terms of either the GPL or the LGPL, and not to allow others to
 * use your version of this file under the terms of the MPL, indicate your
 * decision by deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL or the LGPL. If you do not delete
 * the provisions above, a recipient may use your version of this file under
 * the terms of any one of the MPL, the GPL or the LGPL.
 *
 * ***** END LICENSE BLOCK ***** */

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RavenQuestionnaire.Core.Utility
{
	public static class StringFieldNameSortingSupport
	{
		#region Private expression tree helpers

		private static LambdaExpression GenerateSelector<TEntity>(String propertyName, out Type resultType) where TEntity : class
		{
			// Create a parameter to pass into the Lambda expression (Entity => Entity.OrderByField).
			var parameter = Expression.Parameter(typeof(TEntity), "Entity");
			//  create the selector part, but support child properties
			PropertyInfo property;
			Expression propertyAccess;
			if (propertyName.Contains('.'))
			{
				// support to be sorted on child fields.
				String[] childProperties = propertyName.Split('.');
				property = typeof(TEntity).GetProperty(childProperties[0], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				propertyAccess = Expression.MakeMemberAccess(parameter, property);
				for (int i = 1; i < childProperties.Length; i++)
				{
					property = property.PropertyType.GetProperty(childProperties[i], BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
					propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
				}
			}
			else
			{
				property = typeof(TEntity).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				propertyAccess = Expression.MakeMemberAccess(parameter, property);
			}
			resultType = property.PropertyType;
			// Create the order by expression.
			return Expression.Lambda(propertyAccess, parameter);
		}
		private static MethodCallExpression GenerateMethodCall<TEntity>(IQueryable<TEntity> source, string methodName, String fieldName) where TEntity : class
		{
			Type type = typeof(TEntity);
			Type selectorResultType;
			LambdaExpression selector = GenerateSelector<TEntity>(fieldName, out selectorResultType);
			MethodCallExpression resultExp = Expression.Call(typeof(Queryable), methodName,
							new Type[] { type, selectorResultType },
							source.Expression, Expression.Quote(selector));
			return resultExp;
		}
		#endregion
		public static IOrderedQueryable<TEntity> OrderBy<TEntity>(this IQueryable<TEntity> source, string fieldName) where TEntity : class
		{
			MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "OrderBy", fieldName);
			return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
		}

		public static IOrderedQueryable<TEntity> OrderByDescending<TEntity>(this IQueryable<TEntity> source, string fieldName) where TEntity : class
		{
			MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "OrderByDescending", fieldName);
			return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
		}
		public static IOrderedQueryable<TEntity> ThenBy<TEntity>(this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
		{
			MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "ThenBy", fieldName);
			return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
		}
		public static IOrderedQueryable<TEntity> ThenByDescending<TEntity>(this IOrderedQueryable<TEntity> source, string fieldName) where TEntity : class
		{
			MethodCallExpression resultExp = GenerateMethodCall<TEntity>(source, "ThenByDescending", fieldName);
			return source.Provider.CreateQuery<TEntity>(resultExp) as IOrderedQueryable<TEntity>;
		}
		public static IOrderedQueryable<TEntity> OrderUsingSortExpression<TEntity>(this IQueryable<TEntity> source, string sortExpression) where TEntity : class
		{
			String[] orderFields = sortExpression.Split(',');
			IOrderedQueryable<TEntity> result = null;
			for (int currentFieldIndex = 0; currentFieldIndex < orderFields.Length; currentFieldIndex++)
			{
				String[] expressionPart = orderFields[currentFieldIndex].Trim().Split(' ');
				String sortField = expressionPart[0];
				Boolean sortDescending = (expressionPart.Length == 2) && (expressionPart[1].Equals("DESC", StringComparison.OrdinalIgnoreCase));
				if (sortDescending)
				{
					result = currentFieldIndex == 0 ? source.OrderByDescending(sortField) : result.ThenByDescending(sortField);
				}
				else
				{
					result = currentFieldIndex == 0 ? source.OrderBy(sortField) : result.ThenBy(sortField);
				}
			}
			return result;
		}
	}
}
