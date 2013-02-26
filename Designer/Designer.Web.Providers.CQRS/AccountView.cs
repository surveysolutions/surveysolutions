using Designer.Web.Providers.Membership;
using System;
namespace Designer.Web.Providers.Repositories.CQRS
{
    public class AccountView : IMembershipAccount
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
    }
}
