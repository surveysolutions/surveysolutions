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

        public StoredEvent(Guid commitId, string origin, Guid eventId, int sequence, DateTime timeStamp, object data)
        {
            CommitId = commitId.ToString();
            Origin = origin;
            EventId = eventId.ToString();
            Sequence = sequence;
            TimeStamp = timeStamp.Ticks;
            Data = GetJsonData(data);
        }

        private string GetJsonData(object payload)
        {
            return JsonConvert.SerializeObject(
                payload, Formatting.None, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    NullValueHandling = NullValueHandling.Ignore,
                    FloatParseHandling = FloatParseHandling.Decimal
                });
        }

        public string CommitId { get; set; }

        public string Origin { get; set; }
       
        public string EventId { get; set; }

        [PrimaryKey]
        public int Sequence { get; set; }

        public long TimeStamp { get; set; }

        public string Data { get; set; }
    }
}