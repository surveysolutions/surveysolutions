using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace Questionnaire.Core.Web.Membership
{
    public class FormsAuthentication : IFormsAuthentication
    {
        #region Implementation of IFormsAuthentication

        public void SignIn(string userName, bool rememberMe)
        {
            System.Web.Security.FormsAuthentication.SetAuthCookie(userName, rememberMe /* createPersistentCookie */);
        }

        public void SignOut()
        {
            System.Web.Security.FormsAuthentication.SignOut();
        }
        public string GetUserIdForCurrentUser()
        {
            MembershipUser u;
            try
            {
                u = System.Web.Security.Membership.GetUser();
            }
            catch (Exception)
            {
                u = null;
            }
            if (u == null) return null;
            byte[] key = (byte[]) u.ProviderUserKey;
            return new System.Text.UTF8Encoding().GetString(key);
        }

        #endregion
    }
}
