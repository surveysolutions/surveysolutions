using System;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class AddRoleToAccountCommand : AccountCommandBase
    {
        public AddRoleToAccountCommand(Guid accountId, SimpleRoleEnum role)
            : base(accountId)
        {
            this.Role = role;
        }

        public SimpleRoleEnum Role { get; private set; }
    }
}