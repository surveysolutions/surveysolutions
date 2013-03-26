// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountPasswordResetTokenChanged.cs" company="">
//   
// </copyright>
// <summary>
//   The account password reset token changed.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Providers.CQRS.Accounts.Events
{
    using System;

    /// <summary>
    /// The account password reset token changed.
    /// </summary>
    public class AccountPasswordResetTokenChanged
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the password reset expiration date.
        /// </summary>
        public DateTime PasswordResetExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the password reset token.
        /// </summary>
        public string PasswordResetToken { get; set; }

        #endregion
    }
}