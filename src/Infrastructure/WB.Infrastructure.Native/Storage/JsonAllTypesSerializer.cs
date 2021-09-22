using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Infrastructure.Native.Storage
{
    public class JsonAllTypesSerializer : IJsonAllTypesSerializer
    {
#pragma warning disable 612, 618
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All,
            DateParseHandling = DateParseHandling.DateTimeOffset,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal,
            Converters = new List<JsonConverter> { new IdentityJsonConverter(), new RosterVectorConverter() },
            SerializationBinder = new OldToNewAssemblyRedirectSerializationBinder()
        };
#pragma warning restore 612, 618

        public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item, jsonSerializerSettings);
        }
        
        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, jsonSerializerSettings);
        }

        public T Deserialize<T>(string payload, Type payloadType)
        {
            return (T)JsonConvert.DeserializeObject(payload, payloadType, jsonSerializerSettings);
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
