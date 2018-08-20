using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel
{
    public class FinishInstallationViewModel : EnumeratorFinishInstallationViewModel
    {
        private readonly IPasswordHasher passwordHasher;
        private readonly IPlainStorage<SupervisorIdentity> supervisorsPlainStorage;
        private readonly ISupervisorSynchronizationService synchronizationService;


        public FinishInstallationViewModel(
            IViewModelNavigationService viewModelNavigationService,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<SupervisorIdentity> interviewersPlainStorage,
            IDeviceSettings deviceSettings,
            ISupervisorSynchronizationService synchronizationService,
            ILogger logger,
            IQRBarcodeScanService qrBarcodeScanService,
            ISerializer serializer,
            IUserInteractionService userInteractionService) 
            : base(viewModelNavigationService, principal, deviceSettings, synchronizationService, 
                logger, qrBarcodeScanService, serializer, userInteractionService)
        {
            this.passwordHasher = passwordHasher;
            this.supervisorsPlainStorage = interviewersPlainStorage;
            this.synchronizationService = synchronizationService;
        }
        
        public override async Task Initialize()
        {
            await base.Initialize();
#if DEBUG
            this.Endpoint = "http://10.0.2.2/headquarters";
            this.UserName = "sup";
            this.Password = "1";
#endif
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
                Email = supervisor.Email,
                PasswordHash = this.passwordHasher.Hash(this.Password),
                Token = credentials.Token
            };

            this.supervisorsPlainStorage.Store(supervisorIdentity);
        }
    }
}
