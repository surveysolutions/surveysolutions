using System;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class SynchronizationFailedAuditLogEntity : BaseAuditLogEntity
    {
        public string ExceptionMessage { get; }
        public string StackTrace { get; }

        public SynchronizationFailedAuditLogEntity(Exception exception) : base(AuditLogEntityType.SynchronizationFailed)
        {
            ExceptionMessage = exception?.Message;
            StackTrace = exception?.StackTrace;
        }
    }
}
