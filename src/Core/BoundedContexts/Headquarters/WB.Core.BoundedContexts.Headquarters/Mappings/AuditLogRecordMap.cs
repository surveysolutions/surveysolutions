using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class AuditLogRecordMap : ClassMapping<AuditLogRecord>
    {
        public AuditLogRecordMap()
        {
            this.Id(x => x.Id, idMap =>
            {
                idMap.Generator(Generators.Increment);
                idMap.Column("Id");
            });

            this.Property(x => x.RecordId);
            this.Property(x => x.ResponsibleId);
            this.Property(x => x.ResponsibleName);
            this.Property(x => x.Time);
            this.Property(x => x.TimeUtc);
            this.Property(x => x.Type);
            this.Property("Payload", m =>
            {
                m.Column("Payload");
            });
        }
    }
}
