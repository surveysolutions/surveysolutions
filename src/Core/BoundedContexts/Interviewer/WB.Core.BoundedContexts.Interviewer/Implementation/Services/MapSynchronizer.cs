using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.MapSynchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class MapSyncProvider : MapSyncProviderBase
    {
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IPasswordHasher passwordHasher;
        private readonly IPrincipal principal;

        public MapSyncProvider(IMapService mapService,
            IOnlineSynchronizationService synchronizationService,
            ILogger logger,
            IHttpStatistician httpStatistician,
            IPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            IAuditLogService auditLogService,
            IEnumeratorSettings enumeratorSettings,
            IUserInteractionService userInteractionService,
            IServiceLocator serviceLocator)
            : base(mapService, synchronizationService, logger, httpStatistician,
                principal, interviewViewRepository, auditLogService, enumeratorSettings, userInteractionService, serviceLocator)
        {
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.passwordHasher = passwordHasher;
            this.principal = principal;
        }

        protected override void UpdatePasswordOfResponsible(RestCredentials credentials)
        {
            var localInterviewer = this.interviewersPlainStorage.FirstOrDefault();
            localInterviewer.PasswordHash = this.passwordHasher.Hash(credentials.Password);
            localInterviewer.Token = credentials.Token;

            this.interviewersPlainStorage.Store(localInterviewer);
            this.principal.SignIn(localInterviewer.Name, credentials.Password, true);
        }
    }
}
