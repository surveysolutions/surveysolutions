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
        private readonly IPlainStorage<SupervisorIdentity> interviewersPlainStorage;

        public LoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<SupervisorIdentity> interviewersPlainStorage,
            IPlainStorage<CompanyLogo> logoStorage,
            ISynchronizationService synchronizationService,
            ILogger logger,
            IAuditLogService auditLogService)
            : base(viewModelNavigationService, principal, passwordHasher, logoStorage, synchronizationService, logger, auditLogService)
        {
            this.interviewersPlainStorage = interviewersPlainStorage;
        }

        public override bool HasUser() => this.interviewersPlainStorage.FirstOrDefault() != null;

        public override string GetUserName()
            => this.interviewersPlainStorage.FirstOrDefault().Name;

        public override void UpdateLocalUser(string token, string passwordHash)
        {
            var localSupervisor = this.interviewersPlainStorage.FirstOrDefault();
            localSupervisor.Token = token;
            localSupervisor.PasswordHash = passwordHash;

            this.interviewersPlainStorage.Store(localSupervisor);
        }
    }
}
