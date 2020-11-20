using NHibernate.Mapping.ByCode.Conformist;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public class CalendarEventMap : ClassMapping<CalendarEvent>
    {
        public CalendarEventMap()
        {
            Id(x => x.PublicKey, mapper => mapper.Column("EventId"));
            
            Property(x => x.Comment);
            Property(x => x.StartUtc);
            Property(x => x.StartTimezone);
            Property(x => x.AssignmentId);
            Property(x => x.InterviewId);
            Property(x => x.IsCompleted);
            Property(x => x.CreatorUserId);
            Property(x => x.UpdateDateUtc);
            Property(x => x.UserName);
            Property(x => x.IsDeleted);
        }
    }
}
