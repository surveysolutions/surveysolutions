using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Supervisor.Services;

namespace WB.UI.Supervisor.ViewModel
{
    public class FinishInstallationViewModel : EnumeratorFinishInstallationViewModel
    {
        private readonly IPasswordHasher passwordHasher;
        private readonly IPlainStorage<SupervisorIdentity> supervisorsPlainStorage;
        private readonly ISupervisorSynchronizationService synchronizationService;

        protected FinishInstallationViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<SupervisorIdentity> interviewersPlainStorage,
            IDeviceSettings deviceSettings,
            ISupervisorSynchronizationService synchronizationService,
            ILogger logger,
            IUserInteractionService userInteractionService) : base(viewModelNavigationService, principal, deviceSettings, synchronizationService, logger, userInteractionService)
        {
            this.passwordHasher = passwordHasher;
            this.supervisorsPlainStorage = interviewersPlainStorage;
            this.synchronizationService = synchronizationService;
        }

        protected override Task RelinkUserToAnotherDeviceAsync(RestCredentials credentials, CancellationToken token) => throw new NotImplementedException();

        protected override async Task SaveUserToLocalStorageAsync(RestCredentials credentials, CancellationToken token)
        {
            var supervisor = await this.synchronizationService.GetSupervisorAsync(credentials, token: token).ConfigureAwait(false);

            var supervisorIdentity = new SupervisorIdentity
            {
                Id = supervisor.Id.FormatGuid(),
                UserId = supervisor.Id,
                Name = this.UserName,
                PasswordHash = this.passwordHasher.Hash(this.Password),
                Token = credentials.Token
            };

            this.supervisorsPlainStorage.Store(supervisorIdentity);
        }
    }
}
