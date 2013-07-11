

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    public class StoredEventWithoutPayload
    {
       
        public string EventSourceId { get; set; }
        
        public string EventId { get; set; }
        public long Sequence { get; set; }
    }
}