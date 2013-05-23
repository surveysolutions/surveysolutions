// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IMembershipHelper.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Shared.Web.Membership
{
    /// <summary>
    /// The MembershipHelper interface.
    /// </summary>
    public interface IMembershipHelper
    {
        #region Public Properties

        /// <summary>
        /// Gets the adminrolename.
        /// </summary>
        string ADMINROLENAME { get; }

        /// <summary>
        /// Gets the userrolename.
        /// </summary>
        string USERROLENAME { get; }

        #endregion
    }
}