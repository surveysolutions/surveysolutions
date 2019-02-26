﻿using System;
using Newtonsoft.Json;

namespace WB.Services.Infrastructure.EventSourcing.Json
{
    public class IdentityJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var identity = (Identity)value;

            writer.WriteStartObject();
            writer.WritePropertyName("id");
            writer.WriteValue(identity.Id);
            writer.WritePropertyName("rosterVector");
            serializer.Serialize(writer, identity.RosterVector, typeof(RosterVector));
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Guid id = Guid.Empty;
            RosterVector vector = RosterVector.Empty;

            while (reader.Read() && reader.TokenType != JsonToken.EndObject && reader.TokenType != JsonToken.EndArray)
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = (string) reader.Value;

                    if (string.Equals(propertyName, "id", StringComparison.OrdinalIgnoreCase))
                    {
                        id = Guid.Parse(reader.ReadAsString());
                        continue;
                    }

                    if (string.Equals(propertyName, "rosterVector", StringComparison.OrdinalIgnoreCase))
                    {
                        reader.Read();
                        vector = serializer.Deserialize<RosterVector>(reader);
                        continue;
                    }
                }
            }

            return new Identity(id, vector);
        }

        private static readonly Type IdentityType = typeof(Identity);

        public override bool CanConvert(Type objectType)
        {
            return objectType == IdentityType;
        }
    }
}
