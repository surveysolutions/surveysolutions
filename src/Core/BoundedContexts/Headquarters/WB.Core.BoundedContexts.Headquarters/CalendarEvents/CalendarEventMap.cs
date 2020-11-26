using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public class CalendarEventMap : ClassMapping<CalendarEvent>
    {
        public CalendarEventMap()
        {
            Id(x => x.PublicKey, mapper => mapper.Column("EventId"));
            Property(x => x.Comment);
            Property(x => x.Start, m=>
            {
                m.Columns(
                    cm => cm.Name("startticks"),
                    cm => cm.Name($"starttimezone"));
                m.Type<NodaTimeZonedDateTimeUserType>();
            });
            
            Property(x => x.AssignmentId);
            Property(x => x.InterviewId);
            Property(x => x.CompletedAtUtc, pm => pm.Type<UtcDateTimeType>());
            Property(x => x.CreatorUserId);
            Property(x => x.UpdateDateUtc,pm => pm.Type<UtcDateTimeType>());
            Property(x => x.DeletedAtUtc,pm => pm.Type<UtcDateTimeType>());
        }
    }
}
