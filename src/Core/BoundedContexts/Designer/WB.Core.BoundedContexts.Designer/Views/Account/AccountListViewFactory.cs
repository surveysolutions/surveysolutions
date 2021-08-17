using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Utils;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    public interface IAccountListViewFactory
    {
        AccountListView Load(AccountListViewInputModel input);
    }

    static class AccountQueries
    {
        private static readonly Dictionary<string, Guid> RoleIds = new Dictionary<string, Guid>
        {
            {nameof(SimpleRoleEnum.Administrator), Guid.Parse("11111111111111111111111111111111")},
            {nameof(SimpleRoleEnum.Administrator), Guid.Parse("22222222222222222222222222222222")}
        };

        public static IQueryable<DesignerIdentityUser> FilterAccounts(this DesignerDbContext dbContext, AccountListViewInputModel input)
        {
            var result = from u in dbContext.Users
                        join r in dbContext.UserRoles on u.Id equals r.UserId
                        select new {User = u, Role = r};
            bool hasName = !string.IsNullOrEmpty(input.Name);
            bool hasEmail = !string.IsNullOrEmpty(input.Email);
            bool hasRole = input.Role != SimpleRoleEnum.Undefined;

            if (hasRole && hasName)
            {
                Guid roleId = RoleIds[input.Role.ToString()];

                result = result.Where(x => x.Role.RoleId == roleId && x.User.UserName == input.Name);
            }
            else if (hasRole)
            {
                Guid roleId = RoleIds[input.Role.ToString()];
                result = result.Where(x => x.Role.RoleId == roleId);
            }
            else if (input.IsNewOnly)
            {
                result = result.Where(x => !x.User.EmailConfirmed);
            }
            else if (hasName)
            {
                result = result.Where(x => x.User.UserName == input.Name);
            }
            else if (hasEmail)
            {
                result = result.Where(x => x.User.Email == input.Email);
            }

            if (!string.IsNullOrWhiteSpace(input.Filter))
            {
                var filterUpperCase = input.Filter.Trim().ToUpper();
                result = result.Where(x => x.User.UserName.ToUpper().Contains(filterUpperCase) || x.User.Email.ToUpper().Contains(filterUpperCase));
            }

            return result.Select(x => x.User).Distinct();
        }
    }

    public class AccountListViewFactory : IAccountListViewFactory
    {
        private readonly DesignerDbContext accounts;

        public AccountListViewFactory(DesignerDbContext accounts)
        {
            this.accounts = accounts;
        }

        public AccountListView Load(AccountListViewInputModel input)
        {
            var count =
                this.accounts.FilterAccounts(input).Count();

            var sortOrder = input.Order.IsNullOrEmpty() ? $"{nameof(DesignerIdentityUser.CreatedAtUtc)} Desc" : input.Order;

            var result = this.accounts.FilterAccounts(input)
                            .OrderUsingSortExpression(sortOrder)
                            .Skip((input.Page - 1)*input.PageSize)
                            .Take(input.PageSize)
                            .ToArray();

            return new AccountListView(input.Page, input.PageSize, count, result, input.Order);
        }

        
    }
}
