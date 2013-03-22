// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccountDocument.cs" company="">
//   
// </copyright>
// <summary>
//   RavenDb account document
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer.Providers.Repositories.RavenDb
{
    using System;
    using System.Collections.Generic;

    using WB.UI.Designer.Providers.Membership;
    using WB.UI.Designer.Providers.Roles;

    /// <summary>
    ///     RavenDb account document
    /// </summary>
    public class AccountDocument : IMembershipAccount, IUserWithRoles
    {
        #region Fields

        /// <summary>
        ///     The _roles.
        /// </summary>
        private readonly List<string> _roles = new List<string>();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="AccountDocument" /> class.
        /// </summary>
        public AccountDocument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountDocument"/> class.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        public AccountDocument(IMembershipAccount user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            this.Copy(user);
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets application that the user belongs to
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        ///     Gets or sets a comment about the user.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        ///     A token that can be sent to the user to confirm the account.
        /// </summary>
        public string ConfirmationToken { get; set; }

        /// <summary>
        ///     Gets or sets when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Gets or sets email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     Gets or sets number of times that the user have answered the password question incorrectly since
        ///     <see
        ///         cref="IMembershipAccount.FailedPasswordAnswerWindowAttemptCount" />
        /// </summary>
        public int FailedPasswordAnswerWindowAttemptCount { get; set; }

        /// <summary>
        ///     Gets or sets when the user answered the password question incorrect for the first time.
        /// </summary>
        /// <value>
        ///     DateTime.MinValue if the user has not entered an incorrect answer (or succeded to login again).
        /// </value>
        public DateTime FailedPasswordAnswerWindowStartedAt { get; set; }

        /// <summary>
        ///     Gets or sets number of login attempts since <see cref="IMembershipAccount.FailedPasswordWindowStartedAt" />.
        /// </summary>
        public int FailedPasswordWindowAttemptCount { get; set; }

        /// <summary>
        ///     Gets or sets when the user entered an incorrect password for the first time
        /// </summary>
        /// <value>
        ///     DateTime.MinValue if the user has not entered an incorrect password (or succeded to login again).
        /// </value>
        public DateTime FailedPasswordWindowStartedAt { get; set; }

        /// <summary>
        ///     Gets or sets raven database id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets whether a new user have been approved and may login.
        /// </summary>
        public bool IsConfirmed { get; set; }

        /// <summary>
        ///     Gets or sets if the account has been locked (the user may not login)
        /// </summary>
        public bool IsLockedOut { get; set; }

        /// <summary>
        ///     Gets or sets if the user is online
        /// </summary>
        /// <remarks>
        ///     Caluclated with the help of <see cref="IMembershipAccount.LastActivityAt" />.
        /// </remarks>
        public bool IsOnline { get; set; }

        /// <summary>
        ///     Gets or sets date/time when the user did something on the site
        /// </summary>
        public DateTime LastActivityAt { get; set; }

        /// <summary>
        ///     Gets or sets when the user was locked out.
        /// </summary>
        public DateTime LastLockedOutAt { get; set; }

        /// <summary>
        ///     Gets or sets date/time when the user logged in last.
        /// </summary>
        public DateTime LastLoginAt { get; set; }

        /// <summary>
        ///     Gets or sets when the password were changed last time.
        /// </summary>
        public DateTime LastPasswordChangeAt { get; set; }

        /// <summary>
        ///     Gets or sets password
        /// </summary>
        /// <remarks>
        ///     The state of the password depends on the <seealso cref="IPasswordStrategy" /> that is used.
        /// </remarks>
        public string Password { get; set; }

        /// <summary>
        ///     Gets or sets answer for the <see cref="IMembershipAccount.PasswordQuestion" />.
        /// </summary>
        public string PasswordAnswer { get; set; }

        /// <summary>
        ///     Gets or sets password question that must be answered to reset password
        /// </summary>
        /// <remarks>
        ///     Controlled by the <see cref="IPasswordPolicy.IsPasswordQuestionRequired" /> property.
        /// </remarks>
        public string PasswordQuestion { get; set; }

        /// <summary>
        ///     Gets or sets the password reset expiration date.
        /// </summary>
        public DateTime PasswordResetExpirationDate { get; set; }

        /// <summary>
        ///     Gets or sets the password reset token.
        /// </summary>
        public string PasswordResetToken { get; set; }

        /// <summary>
        ///     Gets or sets the salt if a hashing strategy is used for the password.
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        ///     Gets or sets ID identifying the user
        /// </summary>
        /// <remarks>
        ///     Should be an id in your system (for instance in your database)
        /// </remarks>
        public object ProviderUserKey { get; set; }

        /// <summary>
        ///     Gets a list of all roles that the user is a member of.
        /// </summary>
        public IEnumerable<string> Roles
        {
            get
            {
                return this._roles;
            }
        }

        /// <summary>
        ///     Gets or sets username
        /// </summary>
        public string UserName { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Add a role to the suer
        /// </summary>
        /// <param name="roleName">
        /// Role to add
        /// </param>
        public void AddRole(string roleName)
        {
            if (roleName == null)
            {
                throw new ArgumentNullException("roleName");
            }

            this._roles.Add(roleName);
        }

        /// <summary>
        /// Copy another account
        /// </summary>
        /// <param name="account">
        /// Account to copy
        /// </param>
        public void Copy(IMembershipAccount account)
        {
            if (account == null)
            {
                throw new ArgumentNullException("account");
            }

            this.UserName = account.UserName;
            this.ApplicationName = account.ApplicationName;
            this.Comment = account.Comment;
            this.CreatedAt = account.CreatedAt;
            this.IsConfirmed = account.IsConfirmed;
            this.IsLockedOut = account.IsLockedOut;
            this.IsOnline = account.IsOnline;
            this.LastActivityAt = account.LastActivityAt;
            this.LastLockedOutAt = account.LastLockedOutAt;
            this.LastLoginAt = account.LastLoginAt;
            this.LastPasswordChangeAt = account.LastPasswordChangeAt;
            this.Email = account.Email;
            this.PasswordQuestion = account.PasswordQuestion;
            this.Password = account.Password;
            this.PasswordAnswer = account.PasswordAnswer;
            this.PasswordSalt = account.PasswordSalt;
            this.ProviderUserKey = account.ProviderUserKey;
            this.FailedPasswordAnswerWindowAttemptCount = account.FailedPasswordAnswerWindowAttemptCount;
            this.FailedPasswordAnswerWindowStartedAt = account.FailedPasswordAnswerWindowStartedAt;
            this.FailedPasswordWindowAttemptCount = account.FailedPasswordWindowAttemptCount;
            this.FailedPasswordWindowStartedAt = account.FailedPasswordWindowStartedAt;
        }

        /// <summary>
        /// Check if the user is a member of the specified role
        /// </summary>
        /// <param name="roleName">
        /// Role
        /// </param>
        /// <returns>
        /// true if user belongs to the role; otherwise false.
        /// </returns>
        public bool IsInRole(string roleName)
        {
            return this._roles.Contains(roleName);
        }

        /// <summary>
        /// Remove a role from the user
        /// </summary>
        /// <param name="roleName">
        /// Role to remove
        /// </param>
        public void RemoveRole(string roleName)
        {
            if (roleName == null)
            {
                throw new ArgumentNullException("roleName");
            }

            this._roles.Remove(roleName);
        }

        #endregion
    }
}