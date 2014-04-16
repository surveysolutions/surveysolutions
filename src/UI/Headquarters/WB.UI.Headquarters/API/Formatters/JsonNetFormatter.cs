using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WB.UI.Headquarters.API.Formatters
{
    public class JsonNetFormatter : MediaTypeFormatter
    {
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public JsonNetFormatter(JsonSerializerSettings jsonSerializerSettings)
        {
            this.jsonSerializerSettings = jsonSerializerSettings ?? new JsonSerializerSettings();

            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            JsonSerializer serializer = JsonSerializer.Create(this.jsonSerializerSettings);

            return Task.Factory.StartNew(() =>
            {
                using (var streamWriter = new StreamWriter(writeStream, Encoding.UTF8))
                {
                    using (var jsonTextWriter = new JsonTextWriter(streamWriter))
                    {
                        serializer.Serialize(jsonTextWriter, value);
                    }
                }
            });
        }
    } 
}