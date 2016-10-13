using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    public interface IAccountListViewFactory
    {
        AccountListView Load(AccountListViewInputModel input);
    }

    public class AccountListViewFactory : IAccountListViewFactory
    {
        private readonly IPlainStorageAccessor<Aggregates.User> accounts;

        public AccountListViewFactory(IPlainStorageAccessor<Aggregates.User> accounts)
        {
            this.accounts = accounts;
        }

        public AccountListView Load(AccountListViewInputModel input)
        {
            var count =
                this.accounts.Query(_ => FilterAccounts(_, input).Count());

            var sortOrder = input.Order.IsNullOrEmpty() ? "CreatedAt  Desc" : input.Order;

            var result =
                this.accounts.Query(
                    _ =>
                        FilterAccounts(_, input)
                            .OrderUsingSortExpression(sortOrder)
                            .Skip((input.Page - 1)*input.PageSize)
                            .Take(input.PageSize)
                            .ToArray());

            return new AccountListView(input.Page, input.PageSize, count, result, input.Order);
        }

        private static IQueryable<Aggregates.User> FilterAccounts(IQueryable<Aggregates.User> _, AccountListViewInputModel input)
        {
            IQueryable<Aggregates.User> result = _;
            bool hasName = !string.IsNullOrEmpty(input.Name);
            bool hasEmail = !string.IsNullOrEmpty(input.Email);
            bool hasRole = input.Role != SimpleRoleEnum.Undefined;

            if (hasRole && hasName)
            {
                result = result.Where((x) => x.SimpleRoles.Any(r => r == input.Role) && x.UserName == input.Name);
            }
            else if (hasRole)
            {
                result = result.Where((x) => x.SimpleRoles.Any(r => r == input.Role));
            }
            else if (input.IsNewOnly)
            {
               result = result.Where((x) => !x.IsConfirmed);
            }
            else if (input.IsOnlineOnly)
            {
                result = result.Where((x) => x.IsOnline);
            }
            else if (hasName)
            {
                result = result.Where((x) => x.UserName == input.Name);
            }
            else if (hasEmail)
            {
                result = result.Where( (x) => x.Email == input.Email);
            }

            if (!string.IsNullOrEmpty(input.Filter))
            {
                var filterLowerCase = input.Filter.Trim().ToLower();
                result = result.Where(x => x.UserName.ToLower().Contains(filterLowerCase) || x.Email.ToLower().Contains(filterLowerCase));
            }

            return result;
        }
    }
}