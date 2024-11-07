using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.GeoTracking;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings;

public class GeoTrackingRecordMap : ClassMapping<GeoTrackingRecord>
{
    public GeoTrackingRecordMap()
    {
        Table("geo_tracking_records");

        Id(x => x.Id, IdMapper => IdMapper.Generator(Generators.Identity));
        Property(x => x.InterviewerId, ptp =>
        {
            ptp.Column("interviewer_id");
            ptp.NotNullable(true);
        });
        Property(x => x.AssignmentId, ptp =>
        {
            ptp.Column("assignment_id");
            ptp.NotNullable(true);
        });
        Property(x => x.Start, ptp =>
        {
            ptp.Column("start_date");
            ptp.Type<DateTimeOffsetType>();
            ptp.NotNullable(true);
        });
        Property(x => x.End, ptp =>
        {
            ptp.Column("end_date");
            ptp.Type<DateTimeOffsetType>();
            ptp.NotNullable(false);
        });
        
        List(x => x.Points, listMap =>
            {
                listMap.Table("geo_tracking_points");
                listMap.Index(index => index.Column("position"));
                listMap.Key(keyMap =>
                {
                    keyMap.Column(clm =>
                    {
                        clm.Name("record_id");
                        clm.Index("idx_geo_tracking_points_record_id");
                    });
                    keyMap.ForeignKey("fk_geo_tracking_records__geo_tracking_points");
                });

                listMap.Cascade(Cascade.All | Cascade.DeleteOrphans);
            },
            rel =>
            {
                rel.Component(cmp =>
                {
                    cmp.Property(x => x.Longitude, ptp =>
                    {
                        ptp.Column("longitude");
                        ptp.NotNullable(true);
                    });
                    cmp.Property(x => x.Latitude, ptp =>
                    {
                        ptp.Column("latitude");
                        ptp.NotNullable(true);
                    });
                    cmp.Property(x => x.Time, ptp =>
                    {
                        ptp.Column("time");
                        ptp.Type<DateTimeOffsetType>();
                        ptp.NotNullable(true);
                    });
                });
            });
    }
}

/*public class GeoTrackingPointMap : ClassMapping<GeoTrackingPoint>
{
    public GeoTrackingPointMap()
    {
        Table("geo_tracking_points");

        Id(x => x.Id, IdMapper => IdMapper.Generator(Generators.Identity));
        Property(x => x.GeoTrackingRecordId, ptp =>
        {
            ptp.Column("record_id");
            ptp.NotNullable(true);
        });
        Property(x => x.Longitude, ptp => ptp.NotNullable(true));
        Property(x => x.Latitude, ptp => ptp.NotNullable(true));
        Property(x => x.Time, ptp =>
        {
            ptp.Type<DateTimeOffsetType>();
            ptp.NotNullable(true);
        });
    }
}
*/
