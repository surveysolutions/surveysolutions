using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    [Obsolete("Should be removed as soon as we sure that there is no stored json events with decimal arrays")]
    public class RosterVectorConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vector = (RosterVector)value;

            if (serializer.TypeNameHandling == TypeNameHandling.All)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("$type");
                writer.WriteValue(FullRosterVectorTypeName);
                writer.WritePropertyName("$values");
                writer.WriteStartArray();
                foreach (var coordinate in vector.CoordinatesAsDecimals)
                {
                    writer.WriteValue(coordinate);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            else
            {
                serializer.Serialize(writer, vector.CoordinatesAsDecimals);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var vector = new List<decimal>();
            while (reader.Read() && reader.TokenType != JsonToken.EndObject && reader.TokenType != JsonToken.EndArray)
            {
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

            if (vector.Any())
            {
                return new RosterVector(vector);
            }

            return RosterVector.Empty;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == RosterVectorType;
        }

        private static readonly Type RosterVectorType = typeof(RosterVector);

        private static RosterVectorConverter instance;
        private static readonly string FullRosterVectorTypeName = typeof(RosterVector).FullName + ", " + typeof(RosterVector).GetTypeInfo().Assembly.GetName().Name;
        public static RosterVectorConverter Instance => instance ?? (instance = new RosterVectorConverter());
    }
}