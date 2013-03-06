using WB.UI.Designer.Providers.Membership;
using WB.UI.Designer.Providers.Roles;
using System;
using System.Collections.Generic;

namespace WB.UI.Designer.Providers.Repositories.RavenDb
{
    using WB.UI.Designer.Providers.Membership;
    using WB.UI.Designer.Providers.Roles;

    /// <summary>
    /// RavenDb account document
    /// </summary>
    public class AccountDocument : IMembershipAccount, IUserWithRoles
    {
        private readonly List<string> _roles = new List<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountDocument"/> class.
        /// </summary>
        public AccountDocument()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountDocument"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        public AccountDocument(IMembershipAccount user)
        {
            if (user == null) throw new ArgumentNullException("user");

            Copy(user);
        }

        /// <summary>
        /// Gets or sets raven database id
        /// </summary>
        public string Id { get; set; }

        #region IMembershipAccount Members

        /// <summary>
        /// A token that can be sent to the user to confirm the account.
        /// </summary>
        public string ConfirmationToken { get; set; }
        /// <summary>
        /// Gets or sets ID identifying the user
        /// </summary>
        /// <remarks>
        /// Should be an id in your system (for instance in your database)
        /// </remarks>
        public object ProviderUserKey { get; set; }

        /// <summary>
        /// Gets or sets username
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets application that the user belongs to
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets password
        /// </summary>
        /// <remarks>The state of the password depends on the <seealso cref="IPasswordStrategy"/> that is used.</remarks>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets password question that must be answered to reset password
        /// </summary>
        /// <remarks>
        /// Controlled by the <see cref="IPasswordPolicy.IsPasswordQuestionRequired"/> property.
        /// </remarks>
        public string PasswordQuestion { get; set; }

        /// <summary>
        /// Gets or sets answer for the <see cref="IMembershipAccount.PasswordQuestion"/>.
        /// </summary>
        public string PasswordAnswer { get; set; }

        /// <summary>
        /// Gets or sets the salt if a hashing strategy is used for the password.
        /// </summary>
        public string PasswordSalt { get; set; }

        /// <summary>
        /// Gets or sets a comment about the user.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets date/time when the user logged in last.
        /// </summary>
        public DateTime LastLoginAt { get; set; }

        /// <summary>
        /// Gets or sets whether a new user have been approved and may login.
        /// </summary>
        public bool IsConfirmed { get; set; }

        /// <summary>
        /// Gets or sets when the password were changed last time.
        /// </summary>
        public DateTime LastPasswordChangeAt { get; set; }

        /// <summary>
        /// Gets or sets if the account has been locked (the user may not login)
        /// </summary>
        public bool IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets if the user is online
        /// </summary>
        /// <remarks>
        /// Caluclated with the help of <see cref="IMembershipAccount.LastActivityAt"/>.
        /// </remarks>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Gets or sets when the user was locked out.
        /// </summary>
        public DateTime LastLockedOutAt { get; set; }

        /// <summary>
        /// Gets or sets when the user entered an incorrect password for the first time
        /// </summary>
        /// <value>
        /// DateTime.MinValue if the user has not entered an incorrect password (or succeded to login again).
        /// </value>
        public DateTime FailedPasswordWindowStartedAt { get; set; }

        /// <summary>
        /// Gets or sets number of login attempts since <see cref="IMembershipAccount.FailedPasswordWindowStartedAt"/>.
        /// </summary>
        public int FailedPasswordWindowAttemptCount { get; set; }

        /// <summary>
        /// Gets or sets when the user answered the password question incorrect for the first time.
        /// </summary>
        /// <value>
        /// DateTime.MinValue if the user has not entered an incorrect answer (or succeded to login again).
        /// </value>
        public DateTime FailedPasswordAnswerWindowStartedAt { get; set; }

        /// <summary>
        /// Gets or sets number of times that the user have answered the password question incorrectly since <see cref="IMembershipAccount.FailedPasswordAnswerWindowAttemptCount"/>
        /// </summary>
        public int FailedPasswordAnswerWindowAttemptCount { get; set; }

        /// <summary>
        /// Gets or sets when the user account was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets date/time when the user did something on the site
        /// </summary>
        public DateTime LastActivityAt { get; set; }


        #endregion

        #region IUserWithRoles Members

        /// <summary>
        /// Gets a list of all roles that the user is a member of.
        /// </summary>
        public IEnumerable<string> Roles
        {
            get { return _roles; }
        }

        /// <summary>
        /// Check if the user is a member of the specified role
        /// </summary>
        /// <param name="roleName">Role</param>
        /// <returns>true if user belongs to the role; otherwise false.</returns>
        public bool IsInRole(string roleName)
        {
            return _roles.Contains(roleName);
        }

        #endregion

        /// <summary>
        /// Add a role to the suer
        /// </summary>
        /// <param name="roleName">Role to add</param>
        public void AddRole(string roleName)
        {
            if (roleName == null) throw new ArgumentNullException("roleName");

            _roles.Add(roleName);
        }

        /// <summary>
        /// Remove a role from the user
        /// </summary>
        /// <param name="roleName">Role to remove</param>
        public void RemoveRole(string roleName)
        {
            if (roleName == null) throw new ArgumentNullException("roleName");

            _roles.Remove(roleName);
        }

        /// <summary>
        /// Copy another account
        /// </summary>
        /// <param name="account">Account to copy</param>
        public void Copy(IMembershipAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");
            UserName = account.UserName;
            ApplicationName = account.ApplicationName;
            Comment = account.Comment;
            CreatedAt = account.CreatedAt;
            IsConfirmed = account.IsConfirmed;
            IsLockedOut = account.IsLockedOut;
            IsOnline = account.IsOnline;
            LastActivityAt = account.LastActivityAt;
            LastLockedOutAt = account.LastLockedOutAt;
            LastLoginAt = account.LastLoginAt;
            LastPasswordChangeAt = account.LastPasswordChangeAt;
            Email = account.Email;
            PasswordQuestion = account.PasswordQuestion;
            Password = account.Password;
            PasswordAnswer = account.PasswordAnswer;
            PasswordSalt = account.PasswordSalt;
            ProviderUserKey = account.ProviderUserKey;
            FailedPasswordAnswerWindowAttemptCount = account.FailedPasswordAnswerWindowAttemptCount;
            FailedPasswordAnswerWindowStartedAt = account.FailedPasswordAnswerWindowStartedAt;
            FailedPasswordWindowAttemptCount = account.FailedPasswordWindowAttemptCount;
            FailedPasswordWindowStartedAt = account.FailedPasswordWindowStartedAt;
        }
    }
}