using System;
using Newtonsoft.Json;

namespace WB.UI.Headquarters.Code
{
    /// <summary>
    /// To be used when you're going to output the json data within a script-element on a web page.
    /// </summary>
    /// <remarks>
    ///     https://stackoverflow.com/a/28111588/72174
    /// </remarks>
    public class JsonJavaScriptEncodeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(Microsoft.Security.Application.Encoder.JavaScriptEncode((string)value, true));
        }
    }
}
