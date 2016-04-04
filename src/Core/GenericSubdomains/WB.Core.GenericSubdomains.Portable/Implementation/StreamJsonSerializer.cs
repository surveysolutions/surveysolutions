using System;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class StreamJsonSerializer : IStreamSerializer
    {
        private readonly TypeSerializationSettings defaultTypeSerialization = TypeSerializationSettings.ObjectsOnly;
        private readonly IJsonSerializerSettingsFactory jsonSerializerSettingsFactory;

        public StreamJsonSerializer(IJsonSerializerSettingsFactory jsonSerializerSettingsFactory)
        {
            this.jsonSerializerSettingsFactory = jsonSerializerSettingsFactory;
        }

        public object DeserializeFromStream(Stream stream, Type type, TypeSerializationSettings? typeSerializationSettings = null)
        {
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return JsonSerializer.Create(jsonSerializerSettingsFactory.GetJsonSerializerSettings(typeSerializationSettings ?? defaultTypeSerialization)).Deserialize(jsonTextReader, type);
            }
        }

        public void SerializeToStream(object value, Type type, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer.Create(jsonSerializerSettingsFactory.GetJsonSerializerSettings(defaultTypeSerialization)).Serialize(jsonWriter, value, type);
            }
        }
    }
}
