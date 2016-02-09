using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public class IdentityTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Identity typedValue = (Identity)value;
            
            writer.WriteStartObject();
            writer.WritePropertyName("id");
            writer.WriteValue(typedValue.Id.ToString("N"));
            writer.WritePropertyName("rosterVector");
            writer.WriteStartArray();
            foreach (var vectorCoordinate in typedValue.RosterVector)
            {
                writer.WriteValue(vectorCoordinate.ToString(serializer.Culture));
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            reader.Read();
            reader.Read();
            var guidString = reader.ReadAsString();

            List<decimal> rosterVector = new List<decimal>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.String)
                {
                    var stringValue = (string)reader.Value;
                    decimal value = decimal.Parse(stringValue, serializer.Culture);
                    rosterVector.Add(value);
                }
            }

            var id = Guid.Parse(guidString);
            return new Identity(id, rosterVector.ToArray());
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (Identity);
        }
    }
}