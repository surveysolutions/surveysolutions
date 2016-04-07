using System;
using System.IO;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class JsonAllTypesSerializer : IJsonAllTypesSerializer
    {
        private readonly IJsonSerializerSettingsFactory jsonSerializerSettingsFactory;
        
        public JsonAllTypesSerializer(IJsonSerializerSettingsFactory jsonSerializerSettingsFactory)
        {
            this.jsonSerializerSettingsFactory = jsonSerializerSettingsFactory;
        }

        public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item, this.jsonSerializerSettingsFactory.GetAllTypesJsonSerializerSettings());
        }
        
        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, jsonSerializerSettingsFactory.GetAllTypesJsonSerializerSettings());
        }

        public T Deserialize<T>(byte[] payload)
        {
            try
            {
                var input = new MemoryStream(payload);
                using (var reader = new StreamReader(input))
                {
                    return JsonSerializer.Create(jsonSerializerSettingsFactory.GetAllTypesJsonSerializerSettings()).Deserialize<T>(new JsonTextReader(reader));
                }
            }
            catch (JsonReaderException ex)
            {
                throw new JsonDeserializationException(ex.Message, ex);
            }
        }

        public object DeserializeFromStream(Stream stream, Type type)
        {
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return JsonSerializer.Create(jsonSerializerSettingsFactory.GetAllTypesJsonSerializerSettings()).Deserialize(jsonTextReader, type);
            }
        }

        public byte[] SerializeToByteArray(object payload)
        {
            var output = new MemoryStream();
            using (var writer = new StreamWriter(output))
            {
                JsonSerializer.Create(this.jsonSerializerSettingsFactory.GetAllTypesJsonSerializerSettings()).Serialize(writer, payload);
            }
            return output.ToArray();
        }
        
    }
}
