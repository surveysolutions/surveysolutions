using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace WB.Services.Infrastructure.EventSourcing.Json
{
    public class RosterVectorJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var vector = value as RosterVector;

                if (vector == null)
                    throw new JsonSerializationException("Expected Identity object value");

                var coordinates = (vector.Coordinates as int[]) ?? Array.Empty<int>();

                if (serializer.TypeNameHandling == TypeNameHandling.All)
                {
                    writer.WriteStartObject();
                    writer.WritePropertyName("$type");
                    writer.WriteValue(FullRosterVectorTypeName);
                    writer.WritePropertyName("$values");
                    writer.WriteStartArray();

                    for (int i = 0; i < coordinates.Length; i++)
                    {
                        writer.WriteValue(coordinates[i]);
                    }

                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }
                else
                {
                    serializer.Serialize(writer, coordinates);
                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var vector = new List<int>();

            if (reader.TokenType == JsonToken.StartObject)
            {
                while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType == JsonToken.PropertyName && reader.Value?.ToString() == "$values")
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
                            vector.Add((int)Convert.ToDecimal(reader.Value)); break;
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
