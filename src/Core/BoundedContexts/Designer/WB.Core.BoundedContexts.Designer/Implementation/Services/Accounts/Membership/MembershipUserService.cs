using WebMatrix.WebData;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership
{
    public class MembershipUserService : IMembershipUserService
    {
        private readonly IMembershipHelper helper;
        private readonly IMembershipWebServiceUser webServiceUser;
        private readonly IMembershipWebUser user;

        public MembershipUserService(IMembershipHelper helper, 
            IMembershipWebUser user, 
            IMembershipWebServiceUser webServiceUser)
        {
            this.helper = helper;
            this.user = user;
            this.webServiceUser = webServiceUser;
        }

        public string ADMINROLENAME => this.helper.ADMINROLENAME;

        public IMembershipWebServiceUser WebServiceUser => this.webServiceUser;

        public IMembershipWebUser WebUser => this.user;

        public string USERROLENAME => this.helper.USERROLENAME;

        public void Logout()
        {
            WebSecurity.Logout();
        }
    }
}