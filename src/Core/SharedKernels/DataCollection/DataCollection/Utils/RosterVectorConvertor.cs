using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    [Obsolete("Should be removed as soon as we sure that there is no stored json events with decimal arrays")]
    public class RosterVectorConvertor : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var vector = (RosterVector)value;
            serializer.Serialize(writer, vector.Array);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var vector = new List<decimal>();

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
    }
}