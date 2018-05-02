using System;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog.Entities;

namespace WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog
{
    public class AuditLogRecord
    {
        private T Deserialize<T>(string json) where T : class, IAuditLogEntity
        {
            var entitySerializer = ServiceLocator.Current.GetInstance<ISerializer>();
            return entitySerializer.Deserialize<T>(json);
        }

        public virtual int Id { get; set; }

        public virtual int RecordId { get; set; }
        public virtual Guid? ResponsibleId { get; set; }
        public virtual string ResponsibleName { get; set; }

        public virtual AuditLogEntityType Type { get; set; }

        public virtual DateTime Time { get; set; }

        public virtual DateTime TimeUtc { get; set; }

        protected virtual string Payload { get; set; }

        private IAuditLogEntity EntityObj;
        public virtual IAuditLogEntity Entity
        {
            get
            {
                if (EntityObj != null)
                    return EntityObj;

                switch (Type)
                {
                    case AuditLogEntityType.CloseInterview:
                        EntityObj = Deserialize<CloseInterviewAuditLogEntity>(Payload); break;
                    case AuditLogEntityType.CompleteInterview:
                        EntityObj = Deserialize<CompleteInterviewAuditLogEntity>(Payload); break;
                    case AuditLogEntityType.OpenApplication:
                        EntityObj = Deserialize<OpenApplicationAuditLogEntity>(Payload); break;
                    case AuditLogEntityType.OpenInterview:
                        EntityObj = Deserialize<OpenInterviewAuditLogEntity>(Payload); break;
                    case AuditLogEntityType.CreateInterviewFromAssignment:
                        EntityObj = Deserialize<CreateInterviewAuditLogEntity>(Payload); break;
                    case AuditLogEntityType.Login:
                        EntityObj = Deserialize<LoginAuditLogEntity>(Payload); break;
                    case AuditLogEntityType.Logout:
                        EntityObj = Deserialize<LogoutAuditLogEntity>(Payload); break;
                    case AuditLogEntityType.Relink:
                        EntityObj = Deserialize<RelinkAuditLogEntity>(Payload); break;
                    case AuditLogEntityType.DeleteInterview:
                        EntityObj = Deserialize<DeleteInterviewAuditLogEntity>(Payload); break;
                    case AuditLogEntityType.SynchronizationCanceled:
                        EntityObj = Deserialize<SynchronizationCanceledAuditLogEntity>(Payload); break;
                    case AuditLogEntityType.SynchronizationCompleted:
                        EntityObj = Deserialize<SynchronizationCompletedAuditLogEntity>(Payload); break;
                    case AuditLogEntityType.SynchronizationStarted:
                        EntityObj = Deserialize<SynchronizationStartedAuditLogEntity>(Payload); break;
                    default:
                        throw new ArgumentException($"Unknown audit log type: {Type}");
                }

                return EntityObj;
            }
        }

        public virtual void SetJsonPayload(string json)
        {
            Payload = json;
        }
    }
}
