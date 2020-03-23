using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public abstract class EnumeratorPrincipal : IPrincipal
    {
        private const string LoggedInPreferenceKey = "loggedInUser";

        protected readonly IPasswordHasher passwordHasher;
        private readonly IPreferencesStorage preferencesStorage;

        protected EnumeratorPrincipal(IPasswordHasher passwordHasher, IPreferencesStorage preferencesStorage)
        {
            this.passwordHasher = passwordHasher;
            this.preferencesStorage = preferencesStorage;
        }
        protected abstract IUserIdentity GetUserByName(string userName);
        protected abstract IUserIdentity GetUserById(string userId);

        public bool IsAuthenticated => !string.IsNullOrEmpty(this.GetLoggedInUserName());

        public IUserIdentity CurrentUserIdentity => this.FindIdentityByUsername(this.GetLoggedInUserName());

        private IUserIdentity FindIdentityByUsername(string userName) 
            => userName == null ? null : this.GetUserByName(userName.ToLower());

        private string GetLoggedInUserName() => this.preferencesStorage.Get(LoggedInPreferenceKey);
        private void SetLoggedInUserName(string userName) =>
            this.preferencesStorage.Store(LoggedInPreferenceKey, userName);

        public bool SignIn(string userName, string password, bool staySignedIn)
        {
            this.SignOut();

            var localUser = FindIdentityByUsername(userName); // db query

            if (localUser == null) return false;

            if (localUser.PasswordHash != null)
            {
                var verificationResult = this.passwordHasher.VerifyPassword(localUser.PasswordHash, password);
                if (verificationResult == PasswordVerificationResult.Failed)
                {
                    return false;
                }

                if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    UpdateUserHash(localUser.Id, this.passwordHasher.Hash(password));
                }

                this.SetLoggedInUserName(localUser.Name);
            }

            return this.IsAuthenticated;
        }

        protected abstract void UpdateUserHash(string userId, string hash);

        public bool SignInWithHash(string userName, string passwordHash, bool staySignedIn)
        {
            this.SignOut();

            var localUser = FindIdentityByUsername(userName); // db query

            if (string.Equals(localUser.PasswordHash, passwordHash, StringComparison.Ordinal))
            {
                this.SetLoggedInUserName(localUser.Name);
            }

            return this.IsAuthenticated;
        }

        public void SignOut() => this.SetLoggedInUserName(string.Empty);

        public bool SignIn(string userId, bool staySignedIn)
        {
            var interviewer = this.GetUserById(userId);
            this.SetLoggedInUserName(interviewer.Name);

            return this.IsAuthenticated;
        }
    }
}
