namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetCalendarEventsResponse: ICommunicationMessage
    {
        public List<CalendarEventApiView> CalendarEvents { get; set; }
    }
}