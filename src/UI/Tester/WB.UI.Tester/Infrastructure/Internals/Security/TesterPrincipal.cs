using System;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Tester.Infrastructure.Internals.Security
{
    internal class TesterPrincipal : ITesterPrincipal
    {
        private readonly IPlainStorage<TesterUserIdentity> usersStorage;
        private TesterUserIdentity currentUserIdentity;

        public bool IsAuthenticated => this.currentUserIdentity != null;
        public IUserIdentity CurrentUserIdentity => this.currentUserIdentity;

        public TesterPrincipal(IPlainStorage<TesterUserIdentity> usersStorage)
        {
            this.usersStorage = usersStorage;
            this.currentUserIdentity = usersStorage.FirstOrDefault();
        }

        public bool SignIn(string userName, string password, bool staySignedIn)
        {
            this.currentUserIdentity = new TesterUserIdentity
            {
                Name = userName,
                Password = password,
                UserId = Guid.NewGuid(),
                Id = userName
            };

            if (staySignedIn)
            {
                this.usersStorage.Store(this.currentUserIdentity);
            }

            return this.IsAuthenticated;
        }

        public bool SignInWithHash(string userName, string passwordHash, bool staySignedIn)
        {
            this.currentUserIdentity = new TesterUserIdentity
            {
                Name = userName,
                Password = passwordHash,
                UserId = Guid.NewGuid(),
                Id = userName
            };

            if (staySignedIn)
            {
                this.usersStorage.Store(this.currentUserIdentity);
            }

            return this.IsAuthenticated;
        }

        public void SignOut()
        {
            this.currentUserIdentity = null;
            this.usersStorage.RemoveAll();
        }

        public bool SignIn(string userId, bool staySignedIn)
        {
            this.currentUserIdentity = this.usersStorage.GetById(userId);
            return this.IsAuthenticated;
        }

        public void UseFakeIdentity()
        {
            this.currentUserIdentity = new TesterUserIdentity
            {
                Name = "Anonim",
                Password = "fake",
                UserId = Guid.NewGuid(),
            };
        }

        public void RemoveFakeIdentity()
        {
            this.currentUserIdentity = null;
        }

        public bool IsFakeIdentity => this.currentUserIdentity?.Password == "fake";
    }
}