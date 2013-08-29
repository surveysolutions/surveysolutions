using System;
//using Newtonsoft.Json;
using System.Diagnostics;
using Ncqrs.Eventing.Sourcing;

namespace Ncqrs.Eventing.Storage.RavenDB
{
    using Raven.Imports.Newtonsoft.Json;
    using Raven.Imports.Newtonsoft.Json.Converters;

    [DebuggerDisplay("StoredEvent {Data.GetType().Name}")]
    public class StoredEvent
    {
        public string Id { get; set; }
        public long EventSequence { get; set; }
        public Guid EventSourceId { get; set; }
        public Guid CommitId { get; set; }
        public Guid EventIdentifier { get; set; }
        public DateTime EventTimeStamp { get; set; }
        

        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.All)]
        public object Data { get; set; }

        public string EventType { get; set; }
    }
}