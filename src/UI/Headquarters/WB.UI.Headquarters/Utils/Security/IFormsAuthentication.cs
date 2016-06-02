using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security
{
    /// <summary>
    /// The FormsAuthentication interface.
    /// </summary>
    public interface IFormsAuthentication
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get current user.
        /// </summary>
        /// <returns>
        /// The ???.
        /// </returns>
        UserLight GetCurrentUser();

        /// <summary>
        /// The get user id for current user.
        /// </summary>
        /// <returns>
        /// The System.String.
        /// </returns>
        string GetUserIdForCurrentUser();

        /// <summary>
        /// The sign in.
        /// </summary>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="rememberMe">
        /// The remember me.
        /// </param>
        void SignIn(string userName, bool rememberMe);

        void SignIn(string userName, bool rememberMe, string userData);

        /// <summary>
        /// The sign out.
        /// </summary>
        void SignOut();

        #endregion
    }
}