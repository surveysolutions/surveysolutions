using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class AssignmentGpsMap : ClassMapping<AssignmentGps>
    {
        public AssignmentGpsMap()
        {
            Table("assignment_geo_answers");
            
            Id(x => x.Id, map => map.Generator(Generators.Identity));
            Property(x => x.AssignmentId, mto => mto.Column("assignment_id"));

            Property(x => x.QuestionId);
            Property(x => x.RosterVector);
            Property(x => x.Latitude);
            Property(x => x.Longitude);
            Property(x => x.Timestamp, mapper =>
            {
                mapper.Column("timestamp");
                mapper.Type<DateTimeOffsetType>();
            });
        }
    }
}
