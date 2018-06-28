using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Entities
{
    [ExcludeFromCodeCoverage]
    public class Payload : IPayload
    {
        public Payload(Android.Gms.Nearby.Connection.Payload payload)
        {
            this.NearbyPayload = payload;

            switch (payload.PayloadType)
            {
                case Android.Gms.Nearby.Connection.Payload.Type.Stream: Type = PayloadType.Stream; break;
                case Android.Gms.Nearby.Connection.Payload.Type.File: Type = PayloadType.File; break;
                case Android.Gms.Nearby.Connection.Payload.Type.Bytes: Type = PayloadType.Bytes; break;
            }
        }

        public byte[] Bytes => NearbyPayload.AsBytes();
        public long Id => NearbyPayload.Id;
        public Stream Stream => NearbyPayload.AsStream().AsInputStream();
        public PayloadType Type { get; set; }

        public Android.Gms.Nearby.Connection.Payload NearbyPayload { get; }


        public static IPayload FromStream(Stream stream)
        {
            return new Payload(Android.Gms.Nearby.Connection.Payload.FromStream(stream));
        }

        public static IPayload FromBytes(byte[] bytes)
        {
            return new Payload(Android.Gms.Nearby.Connection.Payload.FromBytes(bytes));
        }

        private TaskCompletionSource<byte[]> tsc = null;

        public Task<byte[]> BytesFromStream => tsc?.Task;

        public async Task ReadStreamAsync()
        {
            tsc = new TaskCompletionSource<byte[]>();

            using (var ms = new MemoryStream())
            {
                await Stream.CopyToAsync(ms);
                tsc.SetResult(ms.ToArray());
            }
        }
    }
}
