using System;
using System.Linq;
using System.Threading.Tasks;
using Nito.AsyncEx;
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

        public async Task<bool> SignInAsync(string usernName, string password, bool staySignedIn)
        {
            if (staySignedIn)
            {
                await this.usersStorage.StoreAsync(new TesterUserIdentity
                {
                    Name = usernName,
                    Password = password,
                    UserId = Guid.NewGuid(),
                    Id = usernName
                }).ConfigureAwait(false);
            }

            this.IsAuthenticated = true;
            this.currentUserIdentity.Name = usernName;
            this.currentUserIdentity.Password = password;

            return this.IsAuthenticated;
        }

        public async Task SignOutAsync()
        {
            var testerUserIdentities = this.usersStorage.LoadAll();
            await this.usersStorage.RemoveAsync(testerUserIdentities).ConfigureAwait(false);

            this.IsAuthenticated = false;
            this.currentUserIdentity.Name = string.Empty;
            this.currentUserIdentity.Password = string.Empty;
        }
    }
}