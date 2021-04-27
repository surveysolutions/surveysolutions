using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Type;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public class CalendarEventMap : ClassMapping<CalendarEvent>
    {
        public CalendarEventMap()
        {
            Id(x => x.PublicKey, mapper => mapper.Column("event_id"));
            Property(x => x.Comment);
            Property(x => x.Start, pm=>
            {
                pm.Columns(cm => cm.Name("start_ticks"), 
                    cm => cm.Name($"start_timezone"));
                pm.Type<NodaTimeZonedDateTimeUserType>();
            });
            Property(x => x.AssignmentId, pm => pm.Column("assignment_id"));
            Property(x => x.InterviewId, pm => pm.Column("interview_id"));
            Property(x => x.InterviewKey, pm => pm.Column("interview_key"));
            Property(x => x.CompletedAtUtc, pm =>
            {
                pm.Type<UtcDateTimeType>();
                pm.Column("completed_at_utc");
            });
            Property(x => x.CreatorUserId, pm => pm.Column("creator_user_id"));
            Property(x => x.UpdateDateUtc,pm =>
            {
                pm.Type<UtcDateTimeType>();
                pm.Column("update_date_utc");
            });
            Property(x => x.DeletedAtUtc,pm =>
            {
                pm.Type<UtcDateTimeType>();
                pm.Column("deleted_at_utc");
            });
            
            ManyToOne(x => x.Creator, mto =>
            {
                mto.Column("creator_user_id");
                mto.Cascade(Cascade.None);
                mto.Update(false);
                mto.Insert(false);
            });
        }
    }
}
