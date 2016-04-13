﻿using System;
using System.IO;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Infrastructure.Native.Storage
{
    public class JsonAllTypesSerializer : IJsonAllTypesSerializer
    {
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public JsonAllTypesSerializer()
        {
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Binder = new OldToNewAssemblyRedirectSerializationBinder()
            };
        }

        public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item, jsonSerializerSettings);
        }
        
        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, jsonSerializerSettings);
        }

        public T Deserialize<T>(byte[] payload)
        {
            try
            {
                var input = new MemoryStream(payload);
                using (var reader = new StreamReader(input))
                {
                    return JsonSerializer.Create(jsonSerializerSettings).Deserialize<T>(new JsonTextReader(reader));
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
                return JsonSerializer.Create(jsonSerializerSettings).Deserialize(jsonTextReader, type);
            }
        }

        public byte[] SerializeToByteArray(object payload)
        {
            var output = new MemoryStream();
            using (var writer = new StreamWriter(output))
            {
                JsonSerializer.Create(jsonSerializerSettings).Serialize(writer, payload);
            }
            return output.ToArray();
        }
        
    }
}
