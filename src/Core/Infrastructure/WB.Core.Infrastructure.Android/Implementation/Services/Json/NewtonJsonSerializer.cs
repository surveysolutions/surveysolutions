using System;
using System.IO;
using Newtonsoft.Json;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Json
{
    public class NewtonJsonSerializer
    {
        private readonly JsonSerializer jsonSerializer;

        public NewtonJsonSerializer()
        {
            this.jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Formatting = Formatting.None
            });
        }

        public T DeserializeAsync<T>(byte[] payload)
        {
            var input = new MemoryStream(payload);
            using (var reader = new StreamReader(input))
            {
                return jsonSerializer.Deserialize<T>(new JsonTextReader(reader));
            }
        }

        public object DeserializeFromStreamAsync(Stream stream, Type type)
        {
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return jsonSerializer.Deserialize(jsonTextReader, type);
            }
        }

        public byte[] SerializeToByteArrayAsync(object payload)
        {
            var output = new MemoryStream();
            using (var writer = new StreamWriter(output))
                jsonSerializer.Serialize(writer, payload);
            return output.ToArray();
        }
    }
}
