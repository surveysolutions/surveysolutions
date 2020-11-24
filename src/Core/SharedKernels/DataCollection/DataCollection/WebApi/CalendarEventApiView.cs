using System;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    
    public class CalendarEventApiView
    {
        public Guid CalendarEventId { get; set; }
        public int? Sequence { get; set; }
        //public Guid? LastEventId { get; set; }
        //public bool IsMarkedAsReceivedByInterviewer { get; set; }
    }
    public class CalendarEventPackageApiView
    {
        public Guid CalendarEventId { get; set; }
        public CalendarEventMetaInfo MetaInfo { get; set; }
        public string Events { get; set; }
    }

    public class CalendarEventMetaInfo
    {
        public DateTimeOffset LastUpdateDateTime { get; set; }
        public Guid ResponsibleId { get; set; }
        
        public Guid? InterviewId  { get; set; }
        public int AssignmentId { get; set; }
    }
}
