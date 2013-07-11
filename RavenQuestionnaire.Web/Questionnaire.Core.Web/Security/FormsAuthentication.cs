namespace Questionnaire.Core.Web.Security
{
    using System;
    using System.Web.Security;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The forms authentication.
    /// </summary>
    public class FormsAuthentication : IFormsAuthentication
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get current user.
        /// </summary>
        /// <returns>
        /// The ???.
        /// </returns>
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

        /// <summary>
        /// The sign in.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="rememberMe">
        /// The remember me.
        /// </param>
        public void SignIn(string userName, bool rememberMe)
        {
            System.Web.Security.FormsAuthentication.SetAuthCookie(userName, rememberMe);
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