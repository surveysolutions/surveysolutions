using Designer.Web.Providers.Roles;

namespace Designer.Web.Providers.CQRS.Accounts.Events
{
    public class AccountRoleRemoved
    {
        public SimpleRoleEnum Role { set; get; }
    }
}
