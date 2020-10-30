using NHibernate.Mapping.ByCode.Conformist;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public class CalendarEventMap : ClassMapping<CalendarEvent>
    {
        public CalendarEventMap()
        {
            Id(x => x.PublicKey, mapper => mapper.Column("PublicKey"));
            
            Property(x => x.Comment);
            Property(x => x.Start);
            Property(x => x.AssignmentId);
            Property(x => x.InterviewId);
            Property(x => x.IsCompleted);
            Property(x => x.UserId);
            Property(x => x.UpdateDate);
            Property(x => x.UserName);
        }
    }
}
