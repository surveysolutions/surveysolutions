using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using WB.Services.Export.Events;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Services
{
    public class FeedEventConverter  : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var feed = JObject.Load(reader);
            var eventTypeName = feed["EventTypeName"].Value<string>();

            var feedEvent = new FeedEvent
            {
                EventSourceId = Guid.Parse(feed[nameof(FeedEvent.EventSourceId)].Value<string>()),
                EventTypeName = feed[nameof(FeedEvent.EventTypeName)].Value<string>(),
                GlobalSequence = feed[nameof(FeedEvent.GlobalSequence)].Value<long>(),
                Sequence = feed[nameof(FeedEvent.Sequence)].Value<int>(),
            };

            if (TypesCache.TryGetValue(eventTypeName, out var eventType))
            {
                feedEvent.Payload = (IEvent) feed[nameof(FeedEvent.Payload)].ToObject(eventType, PayloadSerializer);
            }

            return feedEvent;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(FeedEvent);
        }

        private static readonly JsonSerializer PayloadSerializer = new JsonSerializer
        {
            ContractResolver =  new CamelCasePropertyNamesContractResolver()
        };

        static readonly Dictionary<string, Type> TypesCache = new Dictionary<string, Type>();

        static FeedEventConverter()
        {
            var type = typeof(IEvent);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p));

            foreach (var t in types)
            {
                TypesCache.Add(t.Name, t);
            }
        }
    }
}
