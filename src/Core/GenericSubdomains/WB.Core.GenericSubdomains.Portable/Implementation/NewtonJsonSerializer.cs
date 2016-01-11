using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class NewtonJsonSerializer : ISerializer
    {
        private readonly JsonUtilsSettings jsonUtilsSettings;
        private readonly JsonSerializer jsonSerializer;
        private IJsonSerializerSettingsFactory jsonSerializerSettingsFactory;

        private Dictionary<string, string> assemblyReplacementMapping = new Dictionary<string, string>();

        public NewtonJsonSerializer(IJsonSerializerSettingsFactory jsonSerializerSettingsFactory) 
            : this(jsonSerializerSettingsFactory, new Dictionary<string, string>()) { }

        public NewtonJsonSerializer(IJsonSerializerSettingsFactory jsonSerializerSettingsFactory, Dictionary<string, string> assemblyReplacementMapping)
            : this(jsonSerializerSettingsFactory, new JsonUtilsSettings() { TypeNameHandling = TypeSerializationSettings.ObjectsOnly }, assemblyReplacementMapping)
        {
        }

        public NewtonJsonSerializer(IJsonSerializerSettingsFactory jsonSerializerSettingsFactory, JsonUtilsSettings jsonUtilsSettings) 
            : this(jsonSerializerSettingsFactory, jsonUtilsSettings, new Dictionary<string, string>()) { }

        public NewtonJsonSerializer(
            IJsonSerializerSettingsFactory jsonSerializerSettingsFactory,
            JsonUtilsSettings jsonUtilsSettings,
            Dictionary<string, string> assemblyReplacementMapping)
        {
            this.assemblyReplacementMapping = assemblyReplacementMapping;
            this.jsonSerializerSettingsFactory = jsonSerializerSettingsFactory;
            this.jsonUtilsSettings = jsonUtilsSettings;
            
            this.jsonSerializer =
                JsonSerializer.Create(jsonSerializerSettingsFactory.GetJsonSerializerSettings(this.jsonUtilsSettings.TypeNameHandling));
        }

        public string Serialize(object item)
        {
            return this.Serialize(item, this.jsonUtilsSettings.TypeNameHandling);
        }

        public string Serialize(object item, TypeSerializationSettings typeSerializationSettings)
        {
            return JsonConvert.SerializeObject(item, jsonSerializerSettingsFactory.GetJsonSerializerSettings(typeSerializationSettings));
        }

        public byte[] SerializeToByteArray(object payload)
        {
            var output = new MemoryStream();
            using (var writer = new StreamWriter(output))
                this.jsonSerializer.Serialize(writer, payload);
            return output.ToArray();
        }

        public T Deserialize<T>(string payload)
        {
            return this.Deserialize<T>(payload, TypeSerializationSettings.ObjectsOnly);
        }

        public T Deserialize<T>(string payload, TypeSerializationSettings settings)
        {
            var appliedReplacementPayload = ApplyAssemblyReplacementMapping(payload);
            return JsonConvert.DeserializeObject<T>(appliedReplacementPayload,
                jsonSerializerSettingsFactory.GetJsonSerializerSettings(TypeSerializationSettings.ObjectsOnly));
        }

        public object Deserialize(string payload, Type type, TypeSerializationSettings settings)
        {
            return JsonConvert.DeserializeObject(payload, type, jsonSerializerSettingsFactory.GetJsonSerializerSettings(settings));
        }

        public T Deserialize<T>(byte[] payload)
        {
            try
            {
                var input = new MemoryStream(payload);
                using (var reader = new StreamReader(input))
                {
                    return this.jsonSerializer.Deserialize<T>(new JsonTextReader(reader));
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
                return this.jsonSerializer.Deserialize(jsonTextReader, type);
            }
        }

        public void SerializeToStream(object value, Type type, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                this.jsonSerializer.Serialize(jsonWriter, value, type);
            }
        }

        private string ApplyAssemblyReplacementMapping(string payload)
        {
            var replaceOldAssemblyNames = payload;
            foreach (var item in assemblyReplacementMapping)
            {
                replaceOldAssemblyNames = replaceOldAssemblyNames.Replace(item.Key, item.Value);
            }
            return replaceOldAssemblyNames;

        }
    }
}
