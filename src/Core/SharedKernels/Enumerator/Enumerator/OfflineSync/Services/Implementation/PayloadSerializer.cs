using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation
{
    public class PayloadSerializer : IPayloadSerializer
    {
        private readonly IJsonAllTypesSerializer serializer;

        public PayloadSerializer(IJsonAllTypesSerializer serializer)
        {
            this.serializer = serializer;
        }

        public T FromPayload<T>(byte[] payload)
        {
            using (var ms = new MemoryStream(payload))
            {
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (var sr = new StreamReader(zip))
                    {
                        var json = sr.ReadToEnd();
                        Debug.WriteLine("FromPayload: " + json);
                        return this.serializer.Deserialize<T>(json);
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
                        var json = this.serializer.Serialize(message);
                        Debug.WriteLine("ToPayload: " + json);
                        sw.Write(json);
                    }
                }

                return ms.ToArray();
            }
        }
    }
}
