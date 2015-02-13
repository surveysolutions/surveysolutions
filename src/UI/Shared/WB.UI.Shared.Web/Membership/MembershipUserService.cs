using WebMatrix.WebData;

namespace WB.UI.Shared.Web.Membership
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

        public string ADMINROLENAME
        {
            get
            {
                return this.helper.ADMINROLENAME;
            }
        }

        public IMembershipWebServiceUser WebServiceUser
        {
            get
            {
                return this.webServiceUser;
            }
        }

        public IMembershipWebUser WebUser
        {
            get
            {
                return this.user;
            }
        }

        public string USERROLENAME
        {
            get
            {
                return this.helper.USERROLENAME;
            }
        }

        public void Logout()
        {
            WebSecurity.Logout();
        }
    }
}