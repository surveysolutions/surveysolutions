using System;
using System.Web.Security;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace Questionnaire.Core.Web.Security
{
    public class FormsAuthentication : IFormsAuthentication
    {
        #region Implementation of IFormsAuthentication

        public void SignIn(string userName, bool rememberMe)
        {
            System.Web.Security.FormsAuthentication.SetAuthCookie(userName, rememberMe);
        }

        public void SignOut()
        {
            System.Web.Security.FormsAuthentication.SignOut();
        }

        public string GetUserIdForCurrentUser()
        {
            UserLight user = GetCurrentUser();
            return user != null ? user.Id.ToString() : null;
        }


        public UserLight GetCurrentUser()
        {
            MembershipUser u;
            try
            {
                u = Membership.GetUser();
            }
            catch (Exception)
            {
                u = null;
            }
            if (u == null)
                return null;

            //byte[] key = (byte[])u.ProviderUserKey;

            return new UserLight((Guid)u.ProviderUserKey, u.UserName);

        }


        #endregion
    }
}
