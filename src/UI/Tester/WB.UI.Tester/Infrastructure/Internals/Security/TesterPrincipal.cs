using System;
using System.Linq;
using Nito.AsyncEx.Synchronous;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Tester.Infrastructure.Internals.Security
{
    internal class TesterPrincipal : IPrincipal
    {
        public const string ServiceParameterName = "authentication";

        private readonly IAsyncPlainStorage<TesterUserIdentity> usersStorage;

        private TesterUserIdentity currentUserIdentity;

        public bool IsAuthenticated { get; private set; }
        public IUserIdentity CurrentUserIdentity => this.currentUserIdentity;

        public TesterPrincipal(IAsyncPlainStorage<TesterUserIdentity> usersStorage)
        {
            this.usersStorage = usersStorage;

            this.InitializeIdentity();
        }

        private void InitializeIdentity()
        {
            var testerUserIdentity = this.usersStorage.LoadAll().FirstOrDefault();
            if (testerUserIdentity != null)
            {
                this.IsAuthenticated = true;
                this.currentUserIdentity = testerUserIdentity;
            }
            else
            {
                this.IsAuthenticated = false;
            }
        }

        public bool SignIn(string usernName, string password, bool staySignedIn)
        {
            if (staySignedIn)
            {
                var storeAsync = this.usersStorage.StoreAsync(new TesterUserIdentity
                {
                    Name = usernName,
                    Password = password,
                    UserId = Guid.NewGuid(),
                    Id = usernName
                });
                storeAsync.ConfigureAwait(false);
                storeAsync.WaitAndUnwrapException();
            }

            this.IsAuthenticated = true;
            this.currentUserIdentity.Name = usernName;
            this.currentUserIdentity.Password = password;

            return this.IsAuthenticated;
        }

        public void SignOut()
        {
            var testerUserIdentities = this.usersStorage.LoadAll();
            this.usersStorage.RemoveAsync(testerUserIdentities);

            this.IsAuthenticated = false;
            this.currentUserIdentity.Name = string.Empty;
            this.currentUserIdentity.Password = string.Empty;
        }
    }
}