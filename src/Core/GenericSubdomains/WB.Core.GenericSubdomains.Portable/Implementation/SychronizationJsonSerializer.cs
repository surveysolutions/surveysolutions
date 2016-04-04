using System.IO;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class SychronizationJsonSerializer : ISynchronizationSerializer
    {
        private readonly TypeSerializationSettings defaultTypeSerialization = TypeSerializationSettings.ObjectsOnly;
        private readonly IJsonSerializerSettingsFactory jsonSerializerSettingsFactory;
        
        public SychronizationJsonSerializer(IJsonSerializerSettingsFactory jsonSerializerSettingsFactory)
        {
            this.jsonSerializerSettingsFactory = jsonSerializerSettingsFactory;
        }

        public string Serialize(object item)
        {
            return this.Serialize(item, this.defaultTypeSerialization);
        }

        public string Serialize(object item, TypeSerializationSettings settings)
        {
            return this.Serialize(item, settings, SerializationBinderSettings.NewToOld);
        }

        public string Serialize(object item, TypeSerializationSettings settings, SerializationBinderSettings binderSettings)
        {
            return JsonConvert.SerializeObject(item, this.jsonSerializerSettingsFactory.GetJsonSerializerSettings(settings, binderSettings));
        }

        
        public T Deserialize<T>(string payload)
        {
            return this.Deserialize<T>(payload, this.defaultTypeSerialization);
        }

        public T Deserialize<T>(string payload, TypeSerializationSettings settings)
        {
            return JsonConvert.DeserializeObject<T>(payload, jsonSerializerSettingsFactory.GetJsonSerializerSettings(settings));
        }

        public T Deserialize<T>(byte[] payload)
        {
            try
            {
                var input = new MemoryStream(payload);
                using (var reader = new StreamReader(input))
                {
                    return JsonSerializer.Create(jsonSerializerSettingsFactory.GetJsonSerializerSettings(this.defaultTypeSerialization)).Deserialize<T>(new JsonTextReader(reader));
                }
            }
            catch (JsonReaderException ex)
            {
                throw new JsonDeserializationException(ex.Message, ex);
            }
        }
        
    }
}
