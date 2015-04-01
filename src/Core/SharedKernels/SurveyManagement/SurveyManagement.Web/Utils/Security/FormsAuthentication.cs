using System;
using System.Web;
using System.Web.Security;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security
{
    /// <summary>
    /// The forms authentication.
    /// </summary>
    public class FormsAuthentication : IFormsAuthentication
    {
        #region Public Methods and Operators

        public UserLight GetCurrentUser()
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

            if (u == null)
            {
                return null;
            }

            // byte[] key = (byte[])u.ProviderUserKey;
            return new UserLight((Guid)u.ProviderUserKey, u.UserName);
        }

        /// <summary>
        /// The get user id for current user.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        public string GetUserIdForCurrentUser()
        {
            UserLight user = this.GetCurrentUser();
            return user != null ? user.Id.ToString() : null;
        }


        public void SignIn(string userName, bool rememberMe)
        {
            this.SignIn(userName, rememberMe, string.Empty);
        }
        
        public void SignIn(string userName, bool rememberMe, string userData)
        {
            //System.Web.Security.FormsAuthentication.SetAuthCookie(userName, rememberMe);
 
            HttpCookie authCookie = System.Web.Security.FormsAuthentication.GetAuthCookie(userName, rememberMe);
            FormsAuthenticationTicket ticket = System.Web.Security.FormsAuthentication.Decrypt(authCookie.Value);
            FormsAuthenticationTicket newTicket = new FormsAuthenticationTicket(ticket.Version, ticket.Name, ticket.IssueDate, ticket.Expiration, ticket.IsPersistent, userData);
            authCookie.Value = System.Web.Security.FormsAuthentication.Encrypt(newTicket);
            HttpContext.Current.Response.Cookies.Add(authCookie);

        }

        /// <summary>
        /// The sign out.
        /// </summary>
        public void SignOut()
        {
            System.Web.Security.FormsAuthentication.SignOut();
        }

        #endregion
    }
}