using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class InterviewGpsMap : ClassMapping<InterviewGps>
    {
        public InterviewGpsMap()
        {
            Table("interview_geo_answers");
            ComposedId(m =>
            {
                m.Property(x => x.InterviewId);
                m.Property(x => x.QuestionId);
                m.Property(x => x.RosterVector);
            });
            Property(x => x.Latitude);
            Property(x => x.Longitude);
            Property(x => x.Timestamp, mapper => mapper.Column("timestamp"));
            Property(x => x.IsEnabled);
        }
    }
}
