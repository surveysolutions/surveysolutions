using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class LoginViewModel : EnumeratorLoginViewModel
    {
        private readonly IPlainStorage<SupervisorIdentity> supervsorsPlainStorage;

        public LoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<SupervisorIdentity> supervsorsPlainStorage,
            IPlainStorage<CompanyLogo> logoStorage,
            IRemoteAuthorizationService synchronizationService,
            ILogger logger,
            IAuditLogService auditLogService)
            : base(viewModelNavigationService, principal, passwordHasher, logoStorage, synchronizationService, logger, auditLogService)
        {
            this.supervsorsPlainStorage = supervsorsPlainStorage;
        }

        public override bool HasUser() => this.supervsorsPlainStorage.FirstOrDefault() != null;

        public override string GetUserName()
            => this.supervsorsPlainStorage.FirstOrDefault().Name;

        public override void UpdateLocalUser(string userName, string token, string passwordHash)
        {
            var localSupervisor = this.supervsorsPlainStorage.FirstOrDefault(x => x.Name.ToLower() == userName.ToLower());
            localSupervisor.Token = token;
            localSupervisor.PasswordHash = passwordHash;

            this.supervsorsPlainStorage.Store(localSupervisor);
        }
    }
}
