using System;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class NewtonJsonSerializer : ISerializer
    {
        private readonly JsonUtilsSettings jsonUtilsSettings;
        private IJsonSerializerSettingsFactory jsonSerializerSettingsFactory;

        //assuming it injected not in global scope but pet thread
        private ConcurrentDictionary<TypeSerializationSettings, JsonSerializer> JsonSerializerCache = new ConcurrentDictionary<TypeSerializationSettings, JsonSerializer>();
        
        public NewtonJsonSerializer(IJsonSerializerSettingsFactory jsonSerializerSettingsFactory)
            : this(jsonSerializerSettingsFactory, new JsonUtilsSettings() { TypeNameHandling = TypeSerializationSettings.ObjectsOnly })
        {
        }
        
        public NewtonJsonSerializer(IJsonSerializerSettingsFactory jsonSerializerSettingsFactory, JsonUtilsSettings jsonUtilsSettings)
        {
            this.jsonSerializerSettingsFactory = jsonSerializerSettingsFactory;
            this.jsonUtilsSettings = jsonUtilsSettings;
        }

        private JsonSerializer GetSerializer(TypeSerializationSettings settings)
        {
            JsonSerializer serializer;

            if (!this.JsonSerializerCache.TryGetValue(settings, out serializer))
            {
                serializer = JsonSerializer.Create(jsonSerializerSettingsFactory.GetJsonSerializerSettings(settings));
                this.JsonSerializerCache.TryAdd(settings, serializer);
            }

            return serializer;
        }

        public string Serialize(object item)
        {
            return this.Serialize(item, this.jsonUtilsSettings.TypeNameHandling);

        }

        public string Serialize(object item, SerializationBinderSettings binderSettings)
        {
            return this.Serialize(item, this.jsonUtilsSettings.TypeNameHandling, binderSettings);

        }

        public string Serialize(object item, TypeSerializationSettings typeSerializationSettings, SerializationBinderSettings binderSettings)
        {
            var jsonSerializerSettings = this.jsonSerializerSettingsFactory.GetJsonSerializerSettings(typeSerializationSettings, binderSettings);
            return JsonConvert.SerializeObject(item, jsonSerializerSettings);
        }

        public string Serialize(object item, TypeSerializationSettings typeSerializationSettings)
        {
            return this.Serialize(item, typeSerializationSettings, SerializationBinderSettings.OldToNew);
        }

        public byte[] SerializeToByteArray(object item, TypeSerializationSettings typeSerializationSettings)
        {
            var serializer = this.GetSerializer(typeSerializationSettings);
            var output = new MemoryStream();

            using (var sw = new StreamWriter(output))
            {
                serializer.Serialize(new JsonTextWriter(sw), item);
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
            return JsonConvert.DeserializeObject<T>(payload, jsonSerializerSettingsFactory.GetJsonSerializerSettings(settings));
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

        public object Deserialize(byte[] payload, Type objectType, TypeSerializationSettings typeSerializationSettings)
        {
            var serializer = JsonSerializer.Create(this.jsonSerializerSettingsFactory.GetJsonSerializerSettings(typeSerializationSettings));

            using (MemoryStream ms = new MemoryStream(payload))
            {
                using (var sr = new StreamReader(ms))
                {
                    return serializer.Deserialize(new JsonTextReader(sr), objectType);
                }
            }
        }

        public object DeserializeFromStream(Stream stream, Type type, TypeSerializationSettings? typeSerializationSettings = null)
        {
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return this.GetSerializer(typeSerializationSettings ?? this.jsonUtilsSettings.TypeNameHandling).Deserialize(jsonTextReader, type);
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
    }
}
