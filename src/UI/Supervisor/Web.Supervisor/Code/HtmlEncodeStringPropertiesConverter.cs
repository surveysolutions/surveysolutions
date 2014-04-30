using System;
using Microsoft.Security.Application;
using Newtonsoft.Json;

namespace Web.Supervisor.Code
{
    public class HtmlEncodeStringPropertiesConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Encoder.HtmlEncode(value.ToString()));
        }
    }
}