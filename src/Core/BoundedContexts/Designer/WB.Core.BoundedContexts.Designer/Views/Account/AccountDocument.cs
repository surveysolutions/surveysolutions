using System;
using System.Collections.Generic;
using System.Linq;
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
        public string ApplicationName { get; set; }

        public string Comment { get; set; }

        public string ConfirmationToken { get; set; }

        public DateTime CreatedAt { get; set; }
        
        public string Email { get; set; }

        public int FailedPasswordAnswerWindowAttemptCount { get; set; }

        public DateTime FailedPasswordAnswerWindowStartedAt { get; set; }

        public int FailedPasswordWindowAttemptCount { get; set; }

        public DateTime FailedPasswordWindowStartedAt { get; set; }

        public bool IsConfirmed { get; set; }

        public bool IsLockedOut { get; set; }

        public bool IsOnline { get; set; }

        public DateTime LastActivityAt { get; set; }

        public DateTime LastLockedOutAt { get; set; }

        public DateTime LastLoginAt { get; set; }

        public DateTime LastPasswordChangeAt { get; set; }

        public string Password { get; set; }

        public string PasswordAnswer { get; set; }

        public string PasswordQuestion { get; set; }

        public DateTime PasswordResetExpirationDate { get; set; }

        public string PasswordResetToken { get; set; }

        public string PasswordSalt { get; set; }

        public object ProviderUserKey { get; set; }

        public Guid GetPublicKey()
        {
            if (this.ProviderUserKey == null)
                return Guid.Empty;
            Guid result;
            if (Guid.TryParse(this.ProviderUserKey.ToString(), out result))
                return result;
            return Guid.Empty;
        }

        public List<SimpleRoleEnum> SimpleRoles
        {
            get
            {
                if (simpleRoles == null)
                    simpleRoles = new List<SimpleRoleEnum>();
                return simpleRoles;
            }
            set { simpleRoles = value; }
        }

        private List<SimpleRoleEnum> simpleRoles;

        public string UserName { get; set; }

        public bool IsInRole(string roleName)
        {
            return this.GetRoles().Contains(roleName);
        }
        public IEnumerable<string> GetRoles()
        {
            return this.SimpleRoles.Select(x => Enum.GetName(typeof(SimpleRoleEnum), x));
        }
    }
}