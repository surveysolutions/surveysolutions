using System;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class UploadCalendarEventRequest: ICommunicationMessage
    {
        public CalendarEventPackageApiView CalendarEvent { get; set; }
    }
    
    public class CalendarEventPackageApiView
    {
        public Guid CalendarEventId { get; set; }
        //public InterviewMetaInfo MetaInfo { get; set; }
        public string Events { get; set; }
    }

}