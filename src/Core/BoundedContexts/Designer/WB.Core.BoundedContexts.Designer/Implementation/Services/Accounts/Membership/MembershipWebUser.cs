using System;
using System.Web.Security;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership
{
    public class MembershipWebUser : IMembershipWebUser
    {
        private readonly IMembershipHelper helper;

        public MembershipWebUser(IMembershipHelper helper)
        {
            this.helper = helper;
        }

        public DesignerMembershipUser MembershipUser => (DesignerMembershipUser)System.Web.Security.Membership.GetUser();

        public Guid UserId => Guid.Parse(this.MembershipUser.ProviderUserKey.ToString());

        public string UserName => this.MembershipUser.UserName;

        public bool IsAdmin => Roles.IsUserInRole(this.helper.ADMINROLENAME);

        public bool CanImportOnHq => this.MembershipUser.CanImportOnHq;
    }
}