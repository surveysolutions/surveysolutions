using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.UI.Tester.Infrastructure.Internals.Security
{
    internal class TesterPrincipal : IPrincipal
    {
        private readonly IAsyncPlainStorage<TesterUserIdentity> usersStorage;
        private readonly IAsyncPlainStorage<QuestionnaireListItem> questionnairesStorage;
        private readonly IAsyncPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage;

        private TesterUserIdentity currentUserIdentity;

        public bool IsAuthenticated => this.currentUserIdentity != null;
        public IUserIdentity CurrentUserIdentity => this.currentUserIdentity;

        public TesterPrincipal(IAsyncPlainStorage<TesterUserIdentity> usersStorage,
            IAsyncPlainStorage<QuestionnaireListItem> questionnairesStorage,
            IAsyncPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage)
        {
            this.usersStorage = usersStorage;
            this.questionnairesStorage = questionnairesStorage;
            this.dashboardLastUpdateStorage = dashboardLastUpdateStorage;

            this.currentUserIdentity = usersStorage.FirstOrDefault();
        }

        public async Task<bool> SignInAsync(string userName, string password, bool staySignedIn)
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
                await this.usersStorage.StoreAsync(this.currentUserIdentity).ConfigureAwait(false);
            }

            return this.IsAuthenticated;
        }

        public async Task SignOutAsync()
        {
            await this.usersStorage.RemoveAsync(this.usersStorage.LoadAll()).ConfigureAwait(false);
            await this.dashboardLastUpdateStorage.RemoveAsync(this.dashboardLastUpdateStorage.LoadAll()).ConfigureAwait(false);
            await this.questionnairesStorage.RemoveAsync(this.questionnairesStorage.LoadAll()).ConfigureAwait(false);

            this.currentUserIdentity = null;
        }
    }
}