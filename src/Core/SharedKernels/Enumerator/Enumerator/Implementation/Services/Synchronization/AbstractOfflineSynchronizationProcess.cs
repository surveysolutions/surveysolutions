using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization
{
    public abstract class AbstractOfflineSynchronizationProcess : AbstractSynchronizationProcess
    {
        protected AbstractOfflineSynchronizationProcess(
            IOfflineSynchronizationService synchronizationService,
            ILogger logger,
            IHttpStatistician httpStatistician,
            IPrincipal principal,
            IPlainStorage<InterviewView> interviewViewRepository,
            IAuditLogService auditLogService,
            IEnumeratorSettings enumeratorSettings,
            IServiceLocator serviceLocator) : base(synchronizationService, logger, httpStatistician, principal,
            interviewViewRepository, auditLogService, enumeratorSettings, serviceLocator)
        {
        }

        protected override Task<string> GetNewPasswordAsync() => Task.FromResult((string)null);

        protected override void WriteToAuditLogStartSyncMessage()
            => this.auditLogService.Write(new SynchronizationStartedAuditLogEntity(SynchronizationType.Offline));
    }
}
