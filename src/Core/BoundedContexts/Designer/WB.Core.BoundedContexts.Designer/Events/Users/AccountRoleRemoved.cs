
namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    using WB.UI.Shared.Web.MembershipProvider.Roles;

    public class AccountRoleRemoved
    {
        public SimpleRoleEnum Role { set; get; }
    }
}
