
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    using WB.UI.Shared.Web.MembershipProvider.Roles;

    public class AccountRoleRemoved : IEvent
    {
        public SimpleRoleEnum Role { set; get; }
    }
}
