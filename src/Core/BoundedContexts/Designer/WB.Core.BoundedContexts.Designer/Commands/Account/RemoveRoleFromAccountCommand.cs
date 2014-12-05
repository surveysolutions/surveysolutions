using System;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class RemoveRoleFromAccountCommand : AccountCommandBase
    {
        public RemoveRoleFromAccountCommand(Guid accountId, SimpleRoleEnum role)
            : base(accountId)
        {
            this.Role = role;
        }

        public SimpleRoleEnum Role { get; private set; }
    }
}