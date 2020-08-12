using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class InterviewGpsMap : ClassMapping<InterviewGps>
    {
        public InterviewGpsMap()
        {
            Table("interview_geo_answers");
            Id(x => x.Id, map => map.Generator(Generators.Identity));

            ManyToOne(x => x.InterviewSummary, mto => mto.Column("interview_id"));

            Property(x => x.QuestionId);
            Property(x => x.RosterVector);
            Property(x => x.Latitude);
            Property(x => x.Longitude);
            Property(x => x.Timestamp, mapper =>
            {
                mapper.Column("timestamp");
                mapper.Type<DateTimeOffsetType>();
            });
            Property(x => x.IsEnabled);
        }
    }
}
