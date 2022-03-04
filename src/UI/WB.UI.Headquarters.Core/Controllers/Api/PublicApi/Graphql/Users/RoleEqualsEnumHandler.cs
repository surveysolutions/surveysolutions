#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using HotChocolate.Configuration;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Language;
using HotChocolate.Language.Visitors;
using HotChocolate.Types;
using HotChocolate.Utilities;
using Main.Core.Entities.SubEntities;
using NHibernate.Util;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    public class RoleEqualsEnumHandler : QueryableEnumEqualsHandler
    {
        private static PropertyInfo RolesPropetyInfo;
        private static PropertyInfo RoleIdPropertyInfo;

        static RoleEqualsEnumHandler()
        {
            RoleIdPropertyInfo = typeof(HqRole).GetProperty(nameof(HqRole.Id)) ?? throw new InvalidCastException();
            RolesPropetyInfo = typeof(HqUser).GetProperty(nameof(HqUser.Roles)) ?? throw new InvalidCastException();
        }
        public RoleEqualsEnumHandler(ITypeConverter typeConverter, InputParser inputParser) 
            : base(typeConverter, inputParser)
        {
        }

        public override Expression HandleOperation(QueryableFilterContext context,
            IFilterOperationField field,
            IValueNode value,
            object? parsedValue)
        {
            if (parsedValue is UserRoles role && context.Scopes.Peek() is QueryableScope userQueryableScope)
            {
                // This builds following lambda: u.Roles.Any(r => r.roleId = guidOfRole)
                var parameter = userQueryableScope.Parameter;
                Expression rolesProperty = Expression.Property(parameter, RolesPropetyInfo);

                var userId = role.ToUserId();
                var roleParameter = Expression.Parameter(typeof(HqRole), "r");
                Expression roleIdProperty = Expression.Property(roleParameter, RoleIdPropertyInfo);
                Expression comparison = Expression.Equal(roleIdProperty, Expression.Constant(userId));
                
                Expression result = FilterExpressionBuilder.Any(typeof(HqRole),
                    rolesProperty,
                    comparison,
                    roleParameter
                );

                return result;
            }
         
            throw new InvalidOperationException();
        }
        

        public override bool CanHandle(ITypeCompletionContext context, IFilterInputTypeDefinition typeDefinition,
            IFilterFieldDefinition fieldDefinition)
        {
            var filterFieldDefinition = fieldDefinition as FilterFieldDefinition;

            var canHandle = filterFieldDefinition?.Type?.ToString()?.Contains("UserRoles") == true && 
                            filterFieldDefinition?.Name == "eq";
            return canHandle; 
        }
    }
}
