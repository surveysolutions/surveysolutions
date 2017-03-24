using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    [Obsolete("Should be removed as soon as we sure that there is no stored json events with decimal arrays")]
    public class IdentityJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var identity = (Identity)value;

            writer.WriteStartObject();
            writer.WritePropertyName("id");
            writer.WriteValue(identity.Id);
            writer.WritePropertyName("rosterVector");
            serializer.Serialize(writer, identity.RosterVector.Array);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Guid id = Guid.Empty;
            var vector = new List<decimal>();

            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var propertyName = reader.Value.ToString();
                    
                    if (string.Equals(propertyName, "id", StringComparison.OrdinalIgnoreCase))
                    {
                        id = Guid.Parse(reader.ReadAsString());
                        continue;
                    }

                    if (string.Equals(propertyName, "rosterVector", StringComparison.OrdinalIgnoreCase))
                    {
                        reader.Read();

                        if (reader.TokenType == JsonToken.StartArray)
                        {
                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                if (reader.TokenType != JsonToken.Comment)
                                {
                                    vector.Add(Convert.ToDecimal(reader.Value));
                                }
                            }
                        }
                        continue;
                    }
                }
            }

            if (vector.Any())
            {
                return new Identity(id, new RosterVector(vector));
            }

            return new Identity(id, RosterVector.Empty);
        }

        private static readonly Type IdentityType = typeof(Identity);

        public override bool CanConvert(Type objectType)
        {
            return objectType == IdentityType;
        }

        private static IdentityJsonConverter instance;
        public static IdentityJsonConverter Instance => instance ?? (instance = new IdentityJsonConverter());
    }
}