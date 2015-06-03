using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Shared.Web.MembershipProvider.Accounts;
using WB.UI.Shared.Web.MembershipProvider.Roles;

namespace WB.Core.BoundedContexts.Designer.Views.Account
{
    /// <summary>
    /// The account document.
    /// </summary>
    public class AccountDocument : IMembershipAccount, IUserWithRoles, IView
    {
        public AccountDocument()
        {
            this.SimpleRoles = new HashSet<SimpleRoleEnum>();
        }

        public virtual string ApplicationName { get; set; }

        public virtual string Comment { get; set; }

        public virtual string ConfirmationToken { get; set; }

        public virtual DateTime CreatedAt { get; set; }

        public virtual string Email { get; set; }

        public virtual int FailedPasswordAnswerWindowAttemptCount { get; set; }

        public virtual DateTime FailedPasswordAnswerWindowStartedAt { get; set; }

        public virtual int FailedPasswordWindowAttemptCount { get; set; }

        public virtual DateTime FailedPasswordWindowStartedAt { get; set; }

        public virtual bool IsConfirmed { get; set; }

        public virtual bool IsLockedOut { get; set; }

        public virtual bool IsOnline { get; set; }

        public virtual DateTime LastActivityAt { get; set; }

        public virtual DateTime LastLockedOutAt { get; set; }

        public virtual DateTime LastLoginAt { get; set; }

        public virtual DateTime LastPasswordChangeAt { get; set; }

        public virtual string Password { get; set; }

        public virtual string PasswordAnswer { get; set; }

        public virtual string PasswordQuestion { get; set; }

        public virtual DateTime PasswordResetExpirationDate { get; set; }

        public virtual string PasswordResetToken { get; set; }

        public virtual string PasswordSalt { get; set; }

        public virtual string UserId { get; set; }

        public virtual Guid ProviderUserKey
        {
            get { return providerUserKey; }
            set
            {
                this.UserId = value.FormatGuid();
                providerUserKey = value;
            }
        }

        private Guid providerUserKey;

        public virtual string UserName { get; set; }

        public virtual ISet<SimpleRoleEnum> SimpleRoles { get; set; }

        public virtual bool IsInRole(string roleName)
        {
            return this.GetRoles().Contains(roleName);
        }
        public virtual IEnumerable<string> GetRoles()
        {
            return this.SimpleRoles.Select(x => Enum.GetName(typeof(SimpleRoleEnum), x));
        }
    }
}