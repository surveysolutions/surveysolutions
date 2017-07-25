using System;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership
{
    public class MembershipHelper : IMembershipHelper
    {
        public MembershipHelper()
        {
            this.AdminRoleName = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.Administrator);
            this.UserRoleName = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.User);
        }

        public string AdminRoleName { get; private set; }

        public string UserRoleName { get; private set; }
    }
}