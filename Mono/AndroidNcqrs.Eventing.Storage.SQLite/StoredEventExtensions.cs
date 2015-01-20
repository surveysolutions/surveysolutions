using System;
using Ncqrs.Eventing;
using Newtonsoft.Json;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    public static class StoredEventExtensions
    {
        public static CommittedEvent ToCommitedEvent(this StoredEvent storedEvent, Guid eventSourceId)
        {
            return new CommittedEvent(Guid.Parse(storedEvent.CommitId), storedEvent.Origin, Guid.Parse(storedEvent.EventId),
                                      eventSourceId, storedEvent.Sequence,
                                      DateTime.FromBinary(storedEvent.TimeStamp), GetObject(storedEvent.Data));
        }

        public static StoredEvent ToStoredEvent(this UncommittedEvent evt)
        {
            return  new StoredEvent(evt.CommitId, evt.Origin, evt.EventIdentifier,evt.EventSequence,evt.EventTimeStamp,evt.Payload);
        }

        private static object GetObject(string json)
        {
            return JsonConvert.DeserializeObject(
                json, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    NullValueHandling = NullValueHandling.Ignore,
                    FloatParseHandling = FloatParseHandling.Decimal
                });
        }
    }
}