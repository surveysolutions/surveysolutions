using Designer.Web.Providers.Roles;

namespace Designer.Web.Providers.CQRS.Accounts.Events
{
    public class AccountRoleAdded
    {
        public SimpleRoleEnum Role { set; get; }
    }
}
