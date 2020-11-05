using System;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public class CalendarEventPackage
    {
        public virtual int Id { get; set; }
        public virtual Guid CalendarEventId { get; set; }
        public virtual Guid ResponsibleId { get; set; }
        public virtual DateTime IncomingDate { get; set; }
        public virtual string Events { get; set; }
    }
}
