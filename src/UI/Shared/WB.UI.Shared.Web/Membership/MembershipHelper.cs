using System;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.UI.Shared.Web.Membership
{
    public class MembershipHelper : IMembershipHelper
    {
        public MembershipHelper()
        {
            this.ADMINROLENAME = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.Administrator);
            this.USERROLENAME = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.User);
        }

        public string ADMINROLENAME { get; private set; }

        public string USERROLENAME { get; private set; }
    }
}