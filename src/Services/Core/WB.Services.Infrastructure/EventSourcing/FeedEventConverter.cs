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
            var @event = new Event();
            
            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = (string) reader.Value;

                    switch (propertyName)
                    {
                        case "$type":
                            @event.EventTypeName = reader.ReadAsString();
                            break;
                        case nameof(Event.EventSourceId):
                            @event.EventSourceId = Guid.Parse(reader.ReadAsString());
                            break;
                        case nameof(Event.GlobalSequence):
                            reader.Read();
                            @event.GlobalSequence = (long) reader.Value ;
                            break;
                        case nameof(Event.Sequence):
                            @event.Sequence = reader.ReadAsInt32() ?? 0;
                            break;
                        case nameof(Event.Payload):
                            reader.Read();
                            if (TypesCache.TryGetValue(@event.EventTypeName, out var eventType))
                            {
                                @event.Payload = (IEvent) PayloadSerializer.Deserialize(reader, eventType);
                            }
                            else
                            {
                                reader.Skip();
                            }
                            break;
                    }
                }
            }

            return @event;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Event);
        }

        private static readonly JsonSerializer PayloadSerializer = new JsonSerializer
        {
            ContractResolver =  new CamelCasePropertyNamesContractResolver(),
            Converters =
            {
                //new RosterVectorReader()
            }
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

    internal class RosterVectorReader : JsonConverter<int[]>
    {
        public override void WriteJson(JsonWriter writer, int[] value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override int[] ReadJson(
            JsonReader reader,
            Type objectType,
            int[] existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var json = JArray.Load(reader);
            return json.Select(j => (int) j.Value<float>()).ToArray();
            //return Array.Empty<int>();
        }
    }
}
