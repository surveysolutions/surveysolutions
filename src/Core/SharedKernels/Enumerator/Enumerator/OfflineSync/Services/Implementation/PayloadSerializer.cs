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

        public Task<T> FromPayloadAsync<T>(byte[] payload)
        {
            using (var ms = new MemoryStream(payload))
            {
                using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (var sr = new StreamReader(zip))
                    {
                        // TODO: Fix to async as soon https://github.com/xamarin/xamarin-android/issues/3397 fixed
                        var json = sr.ReadToEnd();
                        return Task.FromResult(this.serializer.Deserialize<T>(json));
                    }
                }
            }
        }
        
        public Task<byte[]> ToPayloadAsync<T>(T message)
        {
            using (var ms = new MemoryStream())
            {
                using (var zip = new GZipStream(ms, CompressionLevel.Optimal))
                {
                    using (var sw = new StreamWriter(zip))
                    {
                        var json = this.serializer.Serialize(message);
                        
                        // TODO: Fix to async as soon https://github.com/xamarin/xamarin-android/issues/3397 fixed
                        sw.Write(json);
                    }
                }

                return Task.FromResult(ms.ToArray());
            }
        }
    }
}
