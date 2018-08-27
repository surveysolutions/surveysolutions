using System;

namespace WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities
{
    public class SynchronizationFailedAuditLogEntity : BaseAuditLogEntity
    {
        public string ExceptionMessage { get; }
        public string StackTrace { get; }

        public SynchronizationFailedAuditLogEntity(string exceptionMessage, string stackTrace) : base(AuditLogEntityType.SynchronizationFailed)
        {
            ExceptionMessage = exceptionMessage;
            StackTrace = stackTrace;
        }

        public static SynchronizationFailedAuditLogEntity CreateFromException(Exception exception) 
        {
            return new SynchronizationFailedAuditLogEntity(exception?.Message, exception?.StackTrace);
        }
    }
}
