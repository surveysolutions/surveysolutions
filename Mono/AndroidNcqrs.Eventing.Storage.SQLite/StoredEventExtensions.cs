using System;
using Ncqrs.Eventing;
using Newtonsoft.Json;

namespace AndroidNcqrs.Eventing.Storage.SQLite
{
    public static class StoredEventExtensions
    {
        public static CommittedEvent ToCommitedEvent(this StoredEvent storedEvent, Guid eventSourceId)
        {

            return new CommittedEvent(Guid.Parse(storedEvent.CommitId), Guid.Parse(storedEvent.EventId),
                                      eventSourceId, storedEvent.Sequence,
                                      DateTime.FromBinary(storedEvent.TimeStamp), GetObject(storedEvent.Data),
                                      new Version(1, 1, 1, 1));
        }
        public static CommittedEvent ToCommitedEventWithoutPayload(this StoredEventWithoutPayload storedEvent)
        {

            return new CommittedEvent(Guid.Empty, Guid.Parse(storedEvent.EventId),
                                      Guid.Parse(storedEvent.EventSourceId), storedEvent.Sequence,
                                      DateTime.Now, null,
                                      new Version(1, 1, 1, 1));
        }
        public static StoredEvent ToStoredEvent(this UncommittedEvent evt)
        {
            return  new StoredEvent(evt.CommitId,evt.EventIdentifier,evt.EventSequence,evt.EventTimeStamp,evt.Payload,evt.EventVersion);
        }

        private static object GetObject(string json)
        {
            return JsonConvert.DeserializeObject(
                json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects });
        }
    }
}