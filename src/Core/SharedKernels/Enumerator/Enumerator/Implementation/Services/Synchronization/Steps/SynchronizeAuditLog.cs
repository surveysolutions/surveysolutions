using System;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization.Steps
{
    public class SynchronizeAuditLog : SynchronizationStep
    {
        protected readonly IAuditLogSynchronizer auditLogSynchronizer;

        public SynchronizeAuditLog(int sortOrder, 
            IAuditLogSynchronizer auditLogSynchronizer,
            ISynchronizationService synchronizationService,
            ILogger logger) : base(sortOrder, synchronizationService, logger)
        {
            this.auditLogSynchronizer = auditLogSynchronizer ?? throw new ArgumentNullException(nameof(auditLogSynchronizer));
        }

        public override Task ExecuteAsync()
        {
            return this.auditLogSynchronizer.SynchronizeAuditLogAsync(Context.Progress, Context.Statistics,
                Context.CancellationToken);
        }
    }
}
