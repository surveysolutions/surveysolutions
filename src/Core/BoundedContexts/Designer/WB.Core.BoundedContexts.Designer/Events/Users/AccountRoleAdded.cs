
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    using WB.UI.Shared.Web.MembershipProvider.Roles;

    public class AccountRoleAdded : ILiteEvent
    {
        public SimpleRoleEnum Role { set; get; }
    }
}
