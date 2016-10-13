using System;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Commands.Account
{
    [Serializable]
    public class RemoveUserRole : UserCommand
    {
        public RemoveUserRole(Guid userId, SimpleRoleEnum role)
            : base(userId)
        {
            this.Role = role;
        }

        public SimpleRoleEnum Role { get; private set; }
    }
}