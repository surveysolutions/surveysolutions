using System;
using Cirrious.MvvmCross.Plugins.Sqlite;

using Newtonsoft.Json;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    public class StoredEvent
    {
        public StoredEvent()
        {
        }

        public StoredEvent(Guid eventSourceId, Guid commitId, Guid eventId, long sequence, DateTime timeStamp, object data,  Version version)
        {
            EventSourceId = eventSourceId.ToString();
            CommitId = commitId.ToString();
            EventId = eventId.ToString();
            Sequence = sequence;
            TimeStamp = timeStamp.Ticks;
            Data = GetJsonData(data);
            
            Version = version.ToString();
        

        }
        private string GetJsonData(object payload)
        {
            return JsonConvert.SerializeObject(
                payload, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
        }
       
        public string EventSourceId { get; set; }

        public string CommitId { get; set; }
        
        [PrimaryKey]
        public string EventId { get; set; }
        public long Sequence { get; set; }
        public long TimeStamp { get; set; }
        public string Data { get; set; }
        public string Version { get; set; }
        
    }
}