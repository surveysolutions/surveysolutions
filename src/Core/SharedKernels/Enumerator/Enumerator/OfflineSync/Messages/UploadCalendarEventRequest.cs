using System;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class UploadCalendarEventRequest: ICommunicationMessage
    {
        public CalendarEventPackageApiView CalendarEvent { get; set; }
    }
}