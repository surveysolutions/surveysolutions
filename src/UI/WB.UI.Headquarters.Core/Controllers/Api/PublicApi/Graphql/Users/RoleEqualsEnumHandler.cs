#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HotChocolate.Configuration;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Filters.Expressions;
using HotChocolate.Language;
using HotChocolate.Language.Visitors;
using HotChocolate.Utilities;
using Main.Core.Entities.SubEntities;
using NHibernate.Util;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Users
{
    
    // public class RolesObjectHandler : QueryableDefaultFieldHandler
    // {
    //     public override bool CanHandle(ITypeCompletionContext context, IFilterInputTypeDefinition typeDefinition,
    //         IFilterFieldDefinition fieldDefinition)
    //     {
    //         var filterFieldDefinition = fieldDefinition as FilterFieldDefinition;
    //         return filterFieldDefinition?.Name == "role";
    //     }
    //
    //     // public override bool TryHandleEnter(QueryableFilterContext context, IFilterField field, ObjectFieldNode node,
    //     //     out ISyntaxVisitorAction? action)
    //     // {
    //     //     action = SyntaxVisitor.Continue;
    //     //     return true;
    //     // }
    //     //
    //     // public override Expression HandleOperation(QueryableFilterContext context, IFilterOperationField field, IValueNode value,
    //     //     object parsedValue)
    //     // {
    //     //     return null;
    //     // }
    // }


    public class RoleEqualsEnumHandler : QueryableEnumEqualsHandler
    {
        public RoleEqualsEnumHandler(ITypeConverter typeConverter) : base(typeConverter)
        {
        }

        public override bool TryHandleEnter(QueryableFilterContext context, IFilterField field, ObjectFieldNode node,
            out ISyntaxVisitorAction? action)
        {
            var tryHandleEnter = base.TryHandleEnter(context, field, node, out action);
            return tryHandleEnter;
        }

        public override bool TryHandleOperation(QueryableFilterContext context, IFilterOperationField field, ObjectFieldNode node,
            out Expression result)
        {
            var tryHandleOperation = base.TryHandleOperation(context, field, node, out result);
            return tryHandleOperation;
        }

        public override Expression HandleOperation(QueryableFilterContext context,
            IFilterOperationField field,
            IValueNode value,
            object? parsedValue)
        {
            if (parsedValue is UserRoles role)
            {
                // Expression<Func<HqUser, bool>> result = (user) => user.Roles.Any(r => r.Id == role.ToUserId());
                
                var expression1 = (MemberExpression)context.GetInstance();
                var hqUserType = expression1.Member.DeclaringType;

                var parameter = Expression.Parameter(typeof(HqUser), "u");
                
                var roleParameter = Expression.Parameter(typeof(HqRole), "r");
                
                Expression rolesProperty = Expression.Property(parameter, hqUserType.GetProperty(nameof(HqUser.Roles)));

                var userId = role.ToUserId();
                Expression roleIdProperty = Expression.Property(roleParameter, typeof(HqRole).GetProperty(nameof(HqRole.Id)));
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
