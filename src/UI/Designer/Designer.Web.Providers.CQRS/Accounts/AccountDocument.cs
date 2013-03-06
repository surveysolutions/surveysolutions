using System.Collections.Generic;
using Designer.Web.Providers.Membership;
using Designer.Web.Providers.Roles;
using System;
using System.Linq;

namespace Designer.Web.Providers.CQRS.Accounts
{
    public class AccountDocument : IMembershipAccount, IUserWithRoles
    {
        public string ApplicationName { get; set; }
        public string Email { get; set; }
        public string PasswordQuestion { get; set; }
        public string PasswordAnswer { get; set; }
        public string Comment { get; set; }
        public DateTime LastLoginAt { get; set; }
        public bool IsConfirmed { get; set; }
        public DateTime LastPasswordChangeAt { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastLockedOutAt { get; set; }
        public DateTime FailedPasswordWindowStartedAt { get; set; }
        public int FailedPasswordWindowAttemptCount { get; set; }
        public DateTime FailedPasswordAnswerWindowStartedAt { get; set; }
        public int FailedPasswordAnswerWindowAttemptCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivityAt { get; set; }
        public object ProviderUserKey { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string ConfirmationToken { get; set; }
        public Guid PublicKey
        {
            get { return (Guid) ProviderUserKey; }
        }

        public List<SimpleRoleEnum> SimpleRoles { set; get; }

        public IEnumerable<string> Roles
        {
            get { return SimpleRoles.Select(x => Enum.GetName(typeof(SimpleRoleEnum), x)); }
        }

        public bool IsInRole(string roleName)
        {
            return Roles.Contains(roleName);
        }
    }
}
