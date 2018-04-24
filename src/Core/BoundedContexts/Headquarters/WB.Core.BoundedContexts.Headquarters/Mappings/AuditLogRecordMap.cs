using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using ThirdParty.Json.LitJson;
using WB.Core.BoundedContexts.Headquarters.AuditLog;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Views.AuditLog;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    [PlainStorage]
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
            this.Property(x => x.Payload, pm =>
            {
                pm.Lazy(false);
                pm.Type<PostgresEntityJson<IAuditLogEntity>>();
            });
        }
    }
}
