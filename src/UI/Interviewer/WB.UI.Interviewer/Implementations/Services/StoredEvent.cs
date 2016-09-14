using System;
using Newtonsoft.Json;
using SQLite;

namespace WB.UI.Interviewer.Implementations.Services
{
    public class StoredEvent
    {
        public StoredEvent()
        {
        }

        public StoredEvent(Guid commitId, string origin, Guid eventId, int sequence, DateTime timeStamp, object data)
        {
            this.CommitId = commitId.ToString();
            this.Origin = origin;
            this.EventId = eventId.ToString();
            this.Sequence = sequence;
            this.TimeStamp = timeStamp.Ticks;
            this.Data = this.GetJsonData(data);
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