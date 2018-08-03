using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.MapSynchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    public class MapSyncProvider : MapSyncProviderBase
    {
        private readonly IPlainStorage<SupervisorIdentity> supervisorPlainStorage;
        private readonly IPasswordHasher passwordHasher;
        private readonly IPrincipal principal;

        public MapSyncProvider(IMapService mapService, 
            ISynchronizationService synchronizationService, 
            ILogger logger, 
            IHttpStatistician httpStatistician, 
            IUserInteractionService userInteractionService,
            IPrincipal principal, 
            IPasswordHasher passwordHasher, 
            IPlainStorage<SupervisorIdentity> supervisorPlainStorage, 
            IPlainStorage<InterviewView> interviewViewRepository, 
            IAuditLogService auditLogService,
            IEnumeratorSettings enumeratorSettings) 
            : base(mapService, synchronizationService, logger, httpStatistician, userInteractionService, principal, 
                interviewViewRepository, auditLogService, enumeratorSettings)
        {
            this.supervisorPlainStorage = supervisorPlainStorage;
            this.passwordHasher = passwordHasher;
            this.principal = principal;
        }

        protected override void UpdatePasswordOfResponsible(RestCredentials credentials)
        {
            var localSupervisor = this.supervisorPlainStorage.FirstOrDefault();
            localSupervisor.PasswordHash = this.passwordHasher.Hash(credentials.Password);
            localSupervisor.Token = credentials.Token;

            this.supervisorPlainStorage.Store(localSupervisor);
            this.principal.SignIn(localSupervisor.Name, credentials.Password, true);
        }
    }
}
