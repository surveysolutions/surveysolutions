namespace WB.UI.Shared.Web.Membership
{
    using System;
    using System.Web.Security;

    /// <summary>
    /// The MembershipUser interface.
    /// </summary>
    public interface IMembershipWebUser
    {
        #region Public Properties

        /// <summary>
        ///     Gets the current user.
        /// </summary>
        MembershipUser MembershipUser { get; }

        /// <summary>
        ///     Gets the current user id.
        /// </summary>
        Guid UserId { get; }

        /// <summary>
        ///     Gets a value indicating whether is admin.
        /// </summary>
        bool IsAdmin { get; }

        #endregion
    }
}