using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
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

        public async Task<T> FromPayloadAsync<T>(byte[] payload)
        {
            var ms = new MemoryStream(payload);
            await using var zip = new GZipStream(ms, CompressionMode.Decompress);
            using var sr = new StreamReader(zip);
            var json = await sr.ReadToEndAsync();
            return this.serializer.Deserialize<T>(json);
        }
        
        public async Task<byte[]> ToPayloadAsync<T>(T message)
        {
            var ms = new MemoryStream();
            await using var zip = new GZipStream(ms, CompressionLevel.Optimal);
            await using var sw = new StreamWriter(zip);
            var json = this.serializer.Serialize(message);
            await sw.WriteAsync(json);

            return ms.ToArray();
        }
    }
}
