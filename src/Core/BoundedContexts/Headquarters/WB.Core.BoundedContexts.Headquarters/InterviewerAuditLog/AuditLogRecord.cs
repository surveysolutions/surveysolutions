using System;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;

namespace WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog
{
    public class AuditLogRecord
    {
        public virtual int Id { get; set; }

        public virtual int RecordId { get; set; }
        public virtual Guid? ResponsibleId { get; set; }
        public virtual string ResponsibleName { get; set; }

        public virtual AuditLogEntityType Type { get; set; }

        public virtual DateTime Time { get; set; }

        public virtual DateTime TimeUtc { get; set; }

        protected virtual string Payload { get; set; }

        public virtual void SetJsonPayload(string json)
        {
            Payload = json;
        }

        public virtual T GetEntity<T>() where T : class, IAuditLogEntity
        {
            var entitySerializer = ServiceLocator.Current.GetInstance<ISerializer>();
            return entitySerializer.Deserialize<T>(Payload);
        }
    }
}
