﻿using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Tester.Infrastructure.Internals.Security
{
    internal class TesterPrincipal : IPrincipal
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

        public bool SignIn(string userName, string passwordHash, bool staySignedIn)
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
        }
    }
}