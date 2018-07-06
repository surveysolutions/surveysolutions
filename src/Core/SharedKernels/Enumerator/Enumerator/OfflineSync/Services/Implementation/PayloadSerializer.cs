using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation
{
    public class PayloadSerializer : IPayloadSerializer
    {
        private readonly JsonSerializer serializer;

        public PayloadSerializer()
        {
            this.serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }

        public T FromPayload<T>(byte[] payload)
        {
            using (var ms = new MemoryStream(payload))
            {
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (var sr = new StreamReader(zip))
                    {
                        using (var jsonTextReader = new JsonTextReader(sr))
                        {
                            return serializer.Deserialize<T>(jsonTextReader);
                        }
                    }
                }
            }
        }

        public byte[] ToPayload<T>(T message)
        {
            using (var ms = new MemoryStream())
            {
                using (var zip = new GZipStream(ms, CompressionLevel.Optimal))
                {
                    using (var sw = new StreamWriter(zip))
                    {
                        using (var jsonWriter = new JsonTextWriter(sw))
                        {
                            serializer.Serialize(jsonWriter, message);
                        }
                    }
                }

                return ms.ToArray();
            }
        }
    }
}
