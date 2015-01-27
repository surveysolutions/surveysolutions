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
            var replaceOldAssemblyNames = json.Replace("Main.Core.Events.AggregateRootEvent, Main.Core", "Main.Core.Events.AggregateRootEvent, WB.Core.Infrastructure");
            foreach (var type in new[] { "NewUserCreated", "UserChanged", "UserLocked", "UserLockedBySupervisor", "UserUnlocked", "UserUnlockedBySupervisor" })
            {
                replaceOldAssemblyNames = replaceOldAssemblyNames.Replace(
                    string.Format("Main.Core.Events.User.{0}, Main.Core", type),
                    string.Format("Main.Core.Events.User.{0}, WB.Core.SharedKernels.DataCollection", type));
            }

            return JsonConvert.DeserializeObject(replaceOldAssemblyNames, 
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    NullValueHandling = NullValueHandling.Ignore,
                    FloatParseHandling = FloatParseHandling.Decimal
                });
        }
    }
}