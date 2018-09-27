using System;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class InterviewerOnlineSynchronizationProcess : AbstractSynchronizationProcess
    {
        private readonly IInterviewerPrincipal principal;
        private readonly IPlainStorage<InterviewerIdentity> interviewersPlainStorage;
        private readonly IPasswordHasher passwordHasher;
        private readonly IInterviewerSynchronizationService interviewerSynchronizationService;

        public InterviewerOnlineSynchronizationProcess(
            IOnlineSynchronizationService synchronizationService,
            IPlainStorage<InterviewerIdentity> interviewersPlainStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            IInterviewerPrincipal principal,
            ILogger logger,
            IPasswordHasher passwordHasher,
            IHttpStatistician httpStatistician,
            IAssignmentDocumentsStorage assignmentsStorage,
            IInterviewerSettings interviewerSettings,
            IAuditLogService auditLogService,
            IUserInteractionService userInteractionService,
            IServiceLocator serviceLocator) : base(synchronizationService, logger, httpStatistician, principal,
            interviewViewRepository, auditLogService, interviewerSettings, serviceLocator, userInteractionService,
            assignmentsStorage)
        {
            this.principal = principal;
            this.interviewersPlainStorage = interviewersPlainStorage;
            this.passwordHasher = passwordHasher;
            this.interviewerSynchronizationService = interviewerSynchronizationService;
        }

        protected override async Task CheckAfterStartSynchronization(CancellationToken cancellationToken)
        {
            var currentSupervisorId = await this.interviewerSynchronizationService.GetCurrentSupervisor(token: cancellationToken, credentials: this.RestCredentials);
            if (currentSupervisorId != this.principal.CurrentUserIdentity.SupervisorId)
            {
                this.UpdateSupervisorOfInterviewer(currentSupervisorId);
            }

            var interviewer = await this.interviewerSynchronizationService.GetInterviewerAsync(this.RestCredentials, token: cancellationToken).ConfigureAwait(false);
            this.UpdateSecurityStampOfInterviewer(interviewer.SecurityStamp);
        }

        private void UpdateSecurityStampOfInterviewer(string securityStamp)
        {
            var localInterviewer = this.interviewersPlainStorage.FirstOrDefault();
            if (localInterviewer.SecurityStamp != securityStamp)
            {
                localInterviewer.SecurityStamp = securityStamp;
                this.interviewersPlainStorage.Store(localInterviewer);
                this.principal.SignInWithHash(localInterviewer.Name, localInterviewer.PasswordHash, true);
            }
        }

        private void UpdateSupervisorOfInterviewer(Guid supervisorId)
        {
            var localInterviewer = this.interviewersPlainStorage.FirstOrDefault();
            localInterviewer.SupervisorId = supervisorId;
            this.interviewersPlainStorage.Store(localInterviewer);
            this.principal.SignInWithHash(localInterviewer.Name, localInterviewer.PasswordHash, true);
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
