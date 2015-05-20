using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Android
{
    public class NewtonJsonUtils : IJsonUtils
    {
        private readonly JsonUtilsSettings jsonUtilsSettings;
        private readonly JsonSerializer jsonSerializer;

        private readonly Dictionary<TypeSerializationSettings, JsonSerializerSettings> jsonSerializerSettingsByTypeNameHandling =
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
                            Formatting = Formatting.None
                        }
                    }
                };

        public NewtonJsonUtils() : this(new JsonUtilsSettings{ TypeNameHandling = TypeSerializationSettings.ObjectsOnly})
        {
        }

        public NewtonJsonUtils(JsonUtilsSettings jsonUtilsSettings)
        {
            this.jsonUtilsSettings = jsonUtilsSettings;
            this.jsonSerializer = JsonSerializer.Create(this.jsonSerializerSettingsByTypeNameHandling[this.jsonUtilsSettings.TypeNameHandling]);
        }

        public string Serialize(object item)
        {
            return this.Serialize(item, this.jsonUtilsSettings.TypeNameHandling);
        }

        public string Serialize(object item, TypeSerializationSettings typeSerializationSettings)
        {
            return JsonConvert.SerializeObject(item, Formatting.None, this.jsonSerializerSettingsByTypeNameHandling[typeSerializationSettings]);
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
            var replaceOldAssemblyNames = ReplaceOldAssemblyNames(payload);
            return JsonConvert.DeserializeObject<T>(replaceOldAssemblyNames, this.jsonSerializerSettingsByTypeNameHandling[TypeSerializationSettings.ObjectsOnly]);
        }

        [Obsolete]
        private static string ReplaceOldAssemblyNames(string payload)
        {
            var replaceOldAssemblyNames = payload;
            replaceOldAssemblyNames = replaceOldAssemblyNames.Replace("Main.Core.Events.AggregateRootEvent, Main.Core", "Main.Core.Events.AggregateRootEvent, WB.Core.Infrastructure");

            foreach (var type in new[] { "NewUserCreated", "UserChanged", "UserLocked", "UserLockedBySupervisor", "UserUnlocked", "UserUnlockedBySupervisor" })
            {
               replaceOldAssemblyNames = replaceOldAssemblyNames.Replace(
                   string.Format("Main.Core.Events.User.{0}, Main.Core", type),
                   string.Format("Main.Core.Events.User.{0}, WB.Core.SharedKernels.DataCollection", type));
            }
            
            return replaceOldAssemblyNames;
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
    }
}
