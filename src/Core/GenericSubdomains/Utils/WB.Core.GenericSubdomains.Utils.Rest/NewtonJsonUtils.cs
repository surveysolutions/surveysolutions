using System;
using System.IO;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Utils.Rest
{
    public class NewtonJsonUtils : IJsonUtils
    {
        private readonly JsonSerializer jsonSerializer;
        private readonly JsonSerializerSettings jsonSerializerSetings;

        public NewtonJsonUtils()
        {
            this.jsonSerializerSetings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Formatting = Formatting.None
            };

            this.jsonSerializer = JsonSerializer.Create(this.jsonSerializerSetings);
        }

        public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item, this.jsonSerializerSetings);
        }

        public byte[] SerializeToByteArray(object payload)
        {
            var output = new MemoryStream();
            using (var writer = new StreamWriter(output))
                jsonSerializer.Serialize(writer, payload);
            return output.ToArray();
        }

        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, this.jsonSerializerSetings);
        }

        public T Deserialize<T>(byte[] payload)
        {
            var input = new MemoryStream(payload);
            using (var reader = new StreamReader(input))
            {
                return jsonSerializer.Deserialize<T>(new JsonTextReader(reader));
            }
        }

        public object DeserializeFromStream(Stream stream, Type type)
        {
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return jsonSerializer.Deserialize(jsonTextReader, type);
            }
        }

        public void SerializeToStream(object value, Type type, Stream stream)
        {
            using (var writer = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonSerializer.Serialize(jsonWriter, value, type);
            }
        }
    }
}
