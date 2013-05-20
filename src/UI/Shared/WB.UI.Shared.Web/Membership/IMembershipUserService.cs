// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMembershipUserService.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Shared.Web.Membership
{
    /// <summary>
    /// The MembershipUserService interface.
    /// </summary>
    public interface IMembershipUserService
    {
        #region Public Properties

        /// <summary>
        ///     The adminrolename.
        /// </summary>
        string ADMINROLENAME { get; }

        /// <summary>
        ///     The userrolename.
        /// </summary>
        string USERROLENAME { get; }

        /// <summary>
        ///     Gets the current web service user.
        /// </summary>
        IMembershipWebServiceUser WebServiceUser { get; }

        /// <summary>
        ///     Gets the current web user.
        /// </summary>
        IMembershipWebUser WebUser { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The logout.
        /// </summary>
        void Logout();

        #endregion
    }
}