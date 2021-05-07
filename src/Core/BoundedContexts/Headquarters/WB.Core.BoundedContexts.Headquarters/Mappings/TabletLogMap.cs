using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Services;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class TabletLogMap : ClassMapping<TabletLog>
    {
        public TabletLogMap()
        {
            Table("tablet_logs");
            Id(x => x.Id, mapper =>
            {
                mapper.Generator(Generators.Identity);
                mapper.Column("id");
            });
            Property(x => x.Content, ptp =>
            {
                ptp.Column("content");
                ptp.Lazy(true);
            });
            Property(x => x.UserName, ptp => ptp.Column("user_name"));
            Property(x => x.DeviceId, ptp => ptp.Column("device_id"));
            Property(x => x.ReceiveDateUtc, ptp => ptp.Column("receive_date_utc"));
        }
    }
}
