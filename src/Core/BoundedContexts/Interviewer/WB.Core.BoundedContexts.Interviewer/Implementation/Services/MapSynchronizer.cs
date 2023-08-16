using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.MapSynchronization;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.MapService;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class MapSyncProvider : MapSyncProviderBase
    {
        private readonly IPasswordHasher passwordHasher;
        private readonly IInterviewerPrincipal interviewerPrincipal;

        public MapSyncProvider(IMapService mapService,
            IOnlineSynchronizationService synchronizationService,
            ILogger logger,
            IHttpStatistician httpStatistician,
            IInterviewerPrincipal principal,
            IPasswordHasher passwordHasher,
            IPlainStorage<InterviewView> interviewViewRepository,
            IAuditLogService auditLogService,
            IEnumeratorSettings enumeratorSettings,
            IUserInteractionService userInteractionService,
            IServiceLocator serviceLocator,
            IDeviceInformationService deviceInformationService,
            IAssignmentDocumentsStorage assignmentsStorage,
            IPermissionsService permissionsService)
            : base(mapService, synchronizationService, logger, httpStatistician,
                principal, interviewViewRepository, auditLogService, enumeratorSettings, userInteractionService, deviceInformationService, serviceLocator, assignmentsStorage, permissionsService)
        {
            this.passwordHasher = passwordHasher;
            this.interviewerPrincipal = principal;
        }

        protected override void UpdatePasswordOfResponsible(RestCredentials credentials)
        {
            var localInterviewer = this.interviewerPrincipal.GetInterviewerByName(credentials.Login);
            if (localInterviewer == null)
                throw new NullReferenceException($"Interviewer with {credentials.Login} not found");
            localInterviewer.PasswordHash = this.passwordHasher.Hash(credentials.Password);
            localInterviewer.Token = credentials.Token;

            this.interviewerPrincipal.SaveInterviewer(localInterviewer);
            this.interviewerPrincipal.SignIn(localInterviewer.Name, credentials.Password, true);
        }
        
        protected override string GetRequiredUpdate(string targetVersion, string appVersion)
        {
            return EnumeratorUIResources.UpgradeRequired 
                   + Environment.NewLine + string.Format(EnumeratorUIResources.HeadquartersVersion, targetVersion) 
                   + Environment.NewLine + string.Format(EnumeratorUIResources.InterviewerVersion, appVersion);
        }

        protected override Task ChangeWorkspaceAndNavigateToItAsync()
            => throw new NotImplementedException("Remove workspace by offline synchronization no supported");
    }
}
