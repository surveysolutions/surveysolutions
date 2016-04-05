namespace WB.UI.Shared.Web.Membership
{
    using System;
    using System.ServiceModel;
    using System.Web.Security;

    /// <summary>
    /// The membership user.
    /// </summary>
    public class MembershipWebServiceUser : IMembershipWebServiceUser
    {
        private readonly IMembershipHelper hepler;

        private  string userName => ServiceSecurityContext.Current.PrimaryIdentity.Name;

        public MembershipWebServiceUser(IMembershipHelper helper)
        {
            this.hepler = helper;
        }

        public MembershipUser MembershipUser => Membership.GetUser(this.userName);

        public Guid UserId => Guid.Parse(this.MembershipUser.ProviderUserKey.ToString());

        public string UserName => this.userName;

        public bool IsAdmin => Roles.IsUserInRole(this.userName, this.hepler.ADMINROLENAME);
    }
}