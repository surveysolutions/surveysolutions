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

            if (serializer.TypeNameHandling == TypeNameHandling.All || serializer.TypeNameHandling == TypeNameHandling.Auto)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("$type");
                writer.WriteValue(FullRosterVectorTypeName);
                writer.WritePropertyName("$values");
                writer.WriteStartArray();
                foreach (var coordinate in vector.Coordinates)
                {
                    writer.WriteValue(coordinate);
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            else
            {
                serializer.Serialize(writer, vector.Coordinates);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var vector = new List<int>();

            if (reader.TokenType == JsonToken.StartObject)
            {
                while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType == JsonToken.PropertyName && reader.Value.ToString() == "$values")
                    {
                        reader.Read();
                        vector = ParseArray(reader);
                    }
                }
            }
            else if (reader.TokenType == JsonToken.StartArray)
            {
                vector = ParseArray(reader);
            }

            return new RosterVector(vector);
        }

        private static List<int> ParseArray(JsonReader reader)
        {
            List<int> vector = new List<int>();

            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
            {
                if (reader.TokenType != JsonToken.Comment)
                {
                    switch (reader.Value)
                    {
                        case Double val: vector.Add((int)val); break;
                        case long val: vector.Add((int)val); break;
                        case int val: vector.Add(val); break;
                        case decimal val: vector.Add((int)val); break;
                        default:
                            vector.Add((int) Convert.ToDecimal(reader.Value)); break;
                    }
                }
            }

            return vector;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == RosterVectorType;
        }

        private static readonly Type RosterVectorType = typeof(RosterVector);
        
        private static readonly string FullRosterVectorTypeName = typeof(RosterVector).FullName + ", " + typeof(RosterVector).GetTypeInfo().Assembly.GetName().Name;
    }
}
