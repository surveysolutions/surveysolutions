using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Infrastructure.Native.Storage
{
    public class IdentityJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var identity = (Identity) value;

            writer.WriteStartObject();
                writer.WritePropertyName("id");
                    writer.WriteValue(identity.Id);
                writer.WritePropertyName("rosterVector");
                    serializer.Serialize(writer, identity.RosterVector);
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Guid id = Guid.Empty;
            List<decimal> vector = new List<decimal>();
            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    if (string.Equals(reader.Value.ToString(), "id", StringComparison.OrdinalIgnoreCase))
                    {
                        id = Guid.Parse(reader.ReadAsString());
                    }

                    if (string.Equals(reader.Value.ToString(), "rosterVector", StringComparison.OrdinalIgnoreCase))
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
                    }
                }
            }

            return new Identity(id, new RosterVector(vector));
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Identity);
        }
    }
}