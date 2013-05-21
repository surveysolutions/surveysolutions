
namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    using WB.UI.Shared.Web.MembershipProvider.Roles;

    public class AccountRoleAdded
    {
        public SimpleRoleEnum Role { set; get; }
    }
}
