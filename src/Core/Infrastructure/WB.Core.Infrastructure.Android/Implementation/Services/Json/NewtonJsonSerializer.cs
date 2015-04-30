using System;
using System.IO;
using Newtonsoft.Json;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Json
{
    internal class NewtonJsonSerializer
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

        public T Deserialize<T>(byte[] payload)
        {
            var input = new MemoryStream(payload);
            using (var reader = new StreamReader(input))
            {
                return jsonSerializer.Deserialize<T>(new JsonTextReader(reader));
            }
        }
    }
}
