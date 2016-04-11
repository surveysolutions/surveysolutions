using System;
using System.IO;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Infrastructure.Shared.Enumerator.Internals
{
    public class PortableJsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public PortableJsonSerializer()
        {
#pragma warning disable 612, 618
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Binder = new PortableOldToNewAssemblyRedirectSerializationBinder()
            };
#pragma warning restore 612, 618
    }

    public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item, jsonSerializerSettings);
        }
        
        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, this.jsonSerializerSettings);
        }
        
        public void SerializeToStream(object value, Type type, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer.Create(jsonSerializerSettings).Serialize(jsonWriter, value, type);
            }
        }
    }
}
