using System;
using System.IO;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class NewtonJsonSerializer : ISerializer
    {
        private readonly IJsonSerializerSettingsFactory jsonSerializerSettingsFactory;

        public NewtonJsonSerializer(IJsonSerializerSettingsFactory jsonSerializerSettingsFactory)
        {
            this.jsonSerializerSettingsFactory = jsonSerializerSettingsFactory;
        }

        public string Serialize(object item)
        {
            var jsonSerializerSettings = this.jsonSerializerSettingsFactory.GetObjectsJsonSerializerSettings();
            return JsonConvert.SerializeObject(item, jsonSerializerSettings);
        }
        
        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, this.jsonSerializerSettingsFactory.GetObjectsJsonSerializerSettings());
        }
        
        public void SerializeToStream(object value, Type type, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer.Create(jsonSerializerSettingsFactory.GetObjectsJsonSerializerSettings()).Serialize(jsonWriter, value, type);
            }
        }
    }
}
