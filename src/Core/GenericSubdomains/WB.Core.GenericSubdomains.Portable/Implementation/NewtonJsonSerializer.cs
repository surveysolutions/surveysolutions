using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;
using Newtonsoft.Json.Bson;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class NewtonJsonSerializer : ISerializer
    {
        private readonly JsonUtilsSettings jsonUtilsSettings;
        private IJsonSerializerSettingsFactory jsonSerializerSettingsFactory;
        private Dictionary<string, string> assemblyReplacementMapping = new Dictionary<string, string>();

        //assuming it injected not in global scope but pet thread
        private Dictionary<TypeSerializationSettings, JsonSerializer> JsonSerializerCache = new Dictionary<TypeSerializationSettings, JsonSerializer>();

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
        }

        private JsonSerializer GetSerializer(TypeSerializationSettings settings)
        {
            JsonSerializer serializer;

            if (!this.JsonSerializerCache.TryGetValue(settings, out serializer))
            {
                serializer = JsonSerializer.Create(jsonSerializerSettingsFactory.GetJsonSerializerSettings(settings));
                this.JsonSerializerCache[settings] = serializer;
            }

            return serializer;
        }

        public string Serialize(object item)
        {
            return this.Serialize(item, this.jsonUtilsSettings.TypeNameHandling);

            //this.SerializeToByteArray(SerializeToByteArray);
        }

        public string Serialize(object item, TypeSerializationSettings typeSerializationSettings)
        {
            return JsonConvert.SerializeObject(item, jsonSerializerSettingsFactory.GetJsonSerializerSettings(typeSerializationSettings));
        }
        
        public byte[] SerializeToByteArray(object item, TypeSerializationSettings typeSerializationSettings, SerializationType serializationType = SerializationType.Json)
        {
            var serializer = this.GetSerializer(typeSerializationSettings);

            var output = new MemoryStream();

            if(serializationType == SerializationType.Bson)
                serializer.Serialize(new BsonWriter(output), item);
            else
            {
                using (var sw = new StreamWriter(output))
                {
                    serializer.Serialize(new JsonTextWriter(sw), item);
                }
                //sw.Flush();
            }
                
            return output.ToArray();
        }

        public byte[] SerializeToByteArray(object payload)
        {
            var output = new MemoryStream();
            using (var writer = new StreamWriter(output))
            {
                this.GetSerializer(this.jsonUtilsSettings.TypeNameHandling).Serialize(writer, payload);
            }
            return output.ToArray();
        }

        public T Deserialize<T>(string payload)
        {
            return this.Deserialize<T>(payload, this.jsonUtilsSettings.TypeNameHandling);
        }

        public T Deserialize<T>(string payload, TypeSerializationSettings settings)
        {
            var appliedReplacementPayload = ApplyAssemblyReplacementMapping(payload);

            return JsonConvert.DeserializeObject<T>(appliedReplacementPayload, jsonSerializerSettingsFactory.GetJsonSerializerSettings(settings));
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
                    return this.GetSerializer(this.jsonUtilsSettings.TypeNameHandling).Deserialize<T>(new JsonTextReader(reader));
                }
            }
            catch (JsonReaderException ex)
            {
                throw new JsonDeserializationException(ex.Message, ex);
            }
        }

        public object Deserialize(byte[] payload, Type objectType, TypeSerializationSettings typeSerializationSettings, SerializationType serializationType = SerializationType.Json)
        {
            var serializer = JsonSerializer.Create(this.jsonSerializerSettingsFactory.GetJsonSerializerSettings(typeSerializationSettings));

            using (MemoryStream ms = new MemoryStream(payload))
            {
                if (serializationType == SerializationType.Bson)
                {
                    return serializer.Deserialize(new BsonReader(ms), objectType);
                }
                else
                {
                    using (var sr = new StreamReader(ms))
                    {
                        return serializer.Deserialize(new JsonTextReader(sr), objectType);
                    }
                }
            }
        }

        public object DeserializeFromStream(Stream stream, Type type)
        {
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return this.GetSerializer(this.jsonUtilsSettings.TypeNameHandling).Deserialize(jsonTextReader, type);
            }
        }

        public void SerializeToStream(object value, Type type, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                this.GetSerializer(this.jsonUtilsSettings.TypeNameHandling).Serialize(jsonWriter, value, type);
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
