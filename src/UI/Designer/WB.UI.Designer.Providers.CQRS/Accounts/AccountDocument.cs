// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountDocument.cs" company="">
//   
// </copyright>
// <summary>
//   The account document.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace WB.UI.Designer.Providers.CQRS.Accounts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using WB.UI.Shared.Web.MembershipProvider.Accounts;
    using WB.UI.Shared.Web.MembershipProvider.Roles;

    /// <summary>
    /// The account document.
    /// </summary>
    public class AccountDocument : IMembershipAccount, IUserWithRoles, IView
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the application name.
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the comment.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the confirmation token.
        /// </summary>
        public string ConfirmationToken { get; set; }

        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the failed password answer window attempt count.
        /// </summary>
        public int FailedPasswordAnswerWindowAttemptCount { get; set; }

        /// <summary>
        /// Gets or sets the failed password answer window started at.
        /// </summary>
        public DateTime FailedPasswordAnswerWindowStartedAt { get; set; }

        /// <summary>
        /// Gets or sets the failed password window attempt count.
        /// </summary>
        public int FailedPasswordWindowAttemptCount { get; set; }

        /// <summary>
        /// Gets or sets the failed password window started at.
        /// </summary>
        public DateTime FailedPasswordWindowStartedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is confirmed.
        /// </summary>
        public bool IsConfirmed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is locked out.
        /// </summary>
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is online.
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Gets or sets the last activity at.
        /// </summary>
        public DateTime LastActivityAt { get; set; }

        /// <summary>
        /// Gets or sets the last locked out at.
        /// </summary>
        public DateTime LastLockedOutAt { get; set; }

        /// <summary>
        /// Gets or sets the last login at.
        /// </summary>
        public DateTime LastLoginAt { get; set; }

        /// <summary>
        /// Gets or sets the last password change at.
        /// </summary>
        public DateTime LastPasswordChangeAt { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the password answer.
        /// </summary>
        public string PasswordAnswer { get; set; }

        /// <summary>
        /// Gets or sets the password question.
        /// </summary>
        public string PasswordQuestion { get; set; }

        /// <summary>
        /// Gets or sets the password reset expiration date.
        /// </summary>
        public DateTime PasswordResetExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the password reset token.
        /// </summary>
        public string PasswordResetToken { get; set; }

        /// <summary>
        /// Gets or sets the password salt.
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// Gets or sets the provider user key.
        /// </summary>
        public object ProviderUserKey { get; set; }

        /// <summary>
        /// Gets the public key.
        /// </summary>
        public Guid PublicKey
        {
            get
            {
                return (Guid)this.ProviderUserKey;
            }
        }

        /// <summary>
        /// Gets the roles.
        /// </summary>
        public IEnumerable<string> Roles
        {
            get
            {
                return this.SimpleRoles.Select(x => Enum.GetName(typeof(SimpleRoleEnum), x));
            }
        }

        /// <summary>
        /// Gets or sets the simple roles.
        /// </summary>
        public List<SimpleRoleEnum> SimpleRoles { get; set; }

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string UserName { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The is in role.
        /// </summary>
        /// <param name="roleName">
        /// The role name.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool IsInRole(string roleName)
        {
            return this.Roles.Contains(roleName);
        }

        #endregion
    }
}