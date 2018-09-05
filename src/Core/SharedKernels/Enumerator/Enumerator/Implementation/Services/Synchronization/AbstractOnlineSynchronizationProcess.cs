using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public abstract class AbstractOnlineSynchronizationProcess : AbstractSynchronizationProcess
    {
        private readonly IUserInteractionService userInteractionService;
        private readonly IPrincipal principal;

        protected AbstractOnlineSynchronizationProcess(
            IOnlineSynchronizationService synchronizationService,
            ILogger logger,
            IHttpStatistician httpStatistician,
            IUserInteractionService userInteractionService,
            IPrincipal principal,
            IPlainStorage<InterviewView> interviewViewRepository,
            IAuditLogService auditLogService,
            IEnumeratorSettings enumeratorSettings,
            IServiceLocator serviceLocator) : base(synchronizationService, logger, httpStatistician, principal,
            interviewViewRepository, auditLogService, enumeratorSettings, serviceLocator)
        {
            this.userInteractionService = userInteractionService;
            this.principal = principal;
        }

        protected override Task<string> GetNewPasswordAsync()
        {
            var message = InterviewerUIResources.Synchronization_UserPassword_Update_Format.FormatString(
                this.principal.CurrentUserIdentity.Name);

            return this.userInteractionService.ConfirmWithTextInputAsync(
                message,
                okButton: UIResources.LoginText,
                cancelButton: InterviewerUIResources.Synchronization_Cancel,
                isTextInputPassword: true);
        }

        protected override void WriteToAuditLogStartSyncMessage()
            => this.auditLogService.Write(new SynchronizationStartedAuditLogEntity(SynchronizationType.Online));
    }
}
