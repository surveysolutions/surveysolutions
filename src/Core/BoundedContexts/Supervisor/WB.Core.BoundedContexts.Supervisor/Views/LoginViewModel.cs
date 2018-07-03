using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Views
{
    public class LoginViewModel : EnumeratorLoginViewModel
    {
        private readonly IPlainStorage<SupervisorIdentity> supervisorsPlainStorage;

        protected LoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<SupervisorIdentity> supervisorsPlainStorage,
            IPlainStorage<CompanyLogo> logoStorage,
            ISynchronizationService synchronizationService,
            ILogger logger,
            IAuditLogService auditLogService)
            : base(viewModelNavigationService, principal, passwordHasher, logoStorage, synchronizationService, logger, auditLogService)
        {
            this.supervisorsPlainStorage = supervisorsPlainStorage;
        }

        public override bool HasUser() => this.supervisorsPlainStorage.FirstOrDefault() != null;

        public override string GetUserName()
            => this.supervisorsPlainStorage.FirstOrDefault().Name;

        public override void UpdateLocalUser(string token, string passwordHash)
        {
            var localInterviewer = this.supervisorsPlainStorage.FirstOrDefault();
            localInterviewer.Token = token;
            localInterviewer.PasswordHash = passwordHash;

            this.supervisorsPlainStorage.Store(localInterviewer);
        }
    }
}
