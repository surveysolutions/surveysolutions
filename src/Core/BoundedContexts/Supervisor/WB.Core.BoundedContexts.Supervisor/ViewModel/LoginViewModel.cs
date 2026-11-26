using System;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
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
        private readonly IPlainStorage<SupervisorIdentity> supervisorsPlainStorage;

        public LoginViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<SupervisorIdentity> supervisorsPlainStorage,
            ICompanyLogoStorage logoStorage,
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

        public override string GetUserLastWorkspace()
            => this.supervisorsPlainStorage.FirstOrDefault().Workspace;

        public override string GetUserPasswordHash()
        {
            var userName = this.UserName;
            if (string.IsNullOrWhiteSpace(userName))
                return null;

            var normalizedUserName = userName.ToLower();
            var supervisor = this.supervisorsPlainStorage.FirstOrDefault(x =>
                x.Name.ToLower() == normalizedUserName);

            return supervisor?.PasswordHash;
        }

        public override void UpdateLocalUser(string userName, string token, string passwordHash)
        {
            var normalizedUserName = userName.ToLower();
            var localSupervisor = this.supervisorsPlainStorage.FirstOrDefault(x =>
                x.Name.ToLower() == normalizedUserName);
            if (localSupervisor == null)
                throw new InvalidOperationException($"Supervisor with {userName} not found");

            localSupervisor.Token = token;
            localSupervisor.PasswordHash = passwordHash;

            this.supervisorsPlainStorage.Store(localSupervisor);
        }
    }
}
