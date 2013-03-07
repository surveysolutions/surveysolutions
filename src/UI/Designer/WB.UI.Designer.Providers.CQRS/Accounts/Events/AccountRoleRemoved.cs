using WB.UI.Designer.Providers.Roles;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    using WB.UI.Designer.Providers.Roles;

    public class AccountRoleRemoved
    {
        public SimpleRoleEnum Role { set; get; }
    }
}
