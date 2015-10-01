using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class NewtonJsonSerializer : ISerializer
    {
        private readonly JsonUtilsSettings jsonUtilsSettings;
        private readonly JsonSerializer jsonSerializer;

        private readonly Dictionary<TypeSerializationSettings, JsonSerializerSettings>
            jsonSerializerSettingsByTypeNameHandling;

        private Dictionary<string, string> assemblyReplacementMapping = new Dictionary<string, string>();

        public NewtonJsonSerializer() : this(new Dictionary<string, string>()) { }

        public NewtonJsonSerializer(Dictionary<string, string> assemblyReplacementMapping)
            : this(new JsonUtilsSettings() { TypeNameHandling = TypeSerializationSettings.ObjectsOnly }, assemblyReplacementMapping)
        {
        }

        public NewtonJsonSerializer(JsonUtilsSettings jsonUtilsSettings) : this(jsonUtilsSettings, new Dictionary<string, string>()) { }

        public NewtonJsonSerializer(JsonUtilsSettings jsonUtilsSettings,
            Dictionary<string, string> assemblyReplacementMapping)
        {
            this.assemblyReplacementMapping = assemblyReplacementMapping;

            this.jsonUtilsSettings = jsonUtilsSettings;

            jsonSerializerSettingsByTypeNameHandling =
               new Dictionary<TypeSerializationSettings, JsonSerializerSettings>()
                {
                    {
                        TypeSerializationSettings.AllTypes, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.All,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal
                        }
                    },
                    {
                        TypeSerializationSettings.ObjectsOnly, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.Objects,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal,
                            Formatting = Formatting.None
                        }
                    },
                    {
                        TypeSerializationSettings.None, new JsonSerializerSettings
                        {
                            TypeNameHandling = TypeNameHandling.None,
                            NullValueHandling = NullValueHandling.Ignore,
                            FloatParseHandling = FloatParseHandling.Decimal,
                            Formatting = Formatting.None                        }
                    }
                };

            this.jsonSerializer =
                JsonSerializer.Create(
                    this.jsonSerializerSettingsByTypeNameHandling[this.jsonUtilsSettings.TypeNameHandling]);
        }

        public string Serialize(object item)
        {
            return this.Serialize(item, this.jsonUtilsSettings.TypeNameHandling);
        }

        public string Serialize(object item, TypeSerializationSettings typeSerializationSettings)
        {
            return JsonConvert.SerializeObject(item, Formatting.None,
                this.jsonSerializerSettingsByTypeNameHandling[typeSerializationSettings]);
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
            var appliedReplacementPayload = ApplyAssemblyReplacementMapping(payload);
            return JsonConvert.DeserializeObject<T>(appliedReplacementPayload,
                this.jsonSerializerSettingsByTypeNameHandling[TypeSerializationSettings.ObjectsOnly]);
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
