using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Json
{
    public class NewtonJsonUtils : IJsonUtils
    {
        private readonly JsonSerializer jsonSerializer;

        public NewtonJsonUtils()
        {
            this.jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Formatting = Formatting.None
            });
        }

        public Task<T> DeserializeAsync<T>(byte[] payload)
        {
            return Task.Run(() =>
            {
                var input = new MemoryStream(payload);
                using (var reader = new StreamReader(input))
                {
                    return jsonSerializer.Deserialize<T>(new JsonTextReader(reader));
                }
            });
        }

        public Task<object> DeserializeFromStreamAsync(Stream stream, Type type)
        {
            return Task.Run(() =>
            {
                using (var sr = new StreamReader(stream))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    return jsonSerializer.Deserialize(jsonTextReader, type);
                }
            });
        }

        public Task<byte[]> SerializeToByteArrayAsync(object payload)
        {
            return Task.Run(() =>
            {
                var output = new MemoryStream();
                using (var writer = new StreamWriter(output))
                    jsonSerializer.Serialize(writer, payload);
                return output.ToArray();
            });
        }
    }
}
