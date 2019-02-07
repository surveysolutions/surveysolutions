using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace WB.Services.Infrastructure.EventSourcing
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

            var feedEvent = new Event
            {
                EventSourceId = Guid.Parse(feed[nameof(Event.EventSourceId)].Value<string>()),
                EventTypeName = feed[nameof(Event.EventTypeName)].Value<string>(),
                GlobalSequence = feed[nameof(Event.GlobalSequence)].Value<long>(),
                Sequence = feed[nameof(Event.Sequence)].Value<int>(),
            };

            if (TypesCache.TryGetValue(eventTypeName, out var eventType))
            {
                feedEvent.Payload = (IEvent) feed[nameof(Event.Payload)].ToObject(eventType, PayloadSerializer);
            }

            return feedEvent;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Event);
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
