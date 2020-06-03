﻿using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Entities;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;

namespace WB.UI.Shared.Enumerator.OfflineSync.Services.Entities
{
    [ExcludeFromCodeCoverage]
    public class Payload : IPayload
    {
        public Payload(Android.Gms.Nearby.Connection.Payload payload, string endpoint)
        {
            this.NearbyPayload = payload;
            Endpoint = endpoint;

            switch (payload.PayloadType)
            {
                case Android.Gms.Nearby.Connection.Payload.Type.Stream: Type = PayloadType.Stream; break;
                case Android.Gms.Nearby.Connection.Payload.Type.File: Type = PayloadType.File; break;
                case Android.Gms.Nearby.Connection.Payload.Type.Bytes: Type = PayloadType.Bytes; break;
            }
        }

        public string Endpoint { get; }

        public byte[] Bytes => NearbyPayload.AsBytes();
        public long Id => NearbyPayload.Id;
        public Stream Stream => NearbyPayload.AsStream().AsInputStream();
        public PayloadType Type { get; set; }

        public Android.Gms.Nearby.Connection.Payload NearbyPayload { get; }


        public static IPayload FromStream(string endpoint, Stream stream)
        {
            return new Payload(Android.Gms.Nearby.Connection.Payload.FromStream(stream), endpoint);
        }

        public static IPayload FromBytes(string endpoint, byte[] bytes)
        {
            return new Payload(Android.Gms.Nearby.Connection.Payload.FromBytes(bytes), endpoint);
        }

        private TaskCompletionSource<byte[]> tsc = null;

        public Task<byte[]> BytesFromStream => tsc?.Task;

        public async Task ReadStreamAsync()
        {
            tsc = new TaskCompletionSource<byte[]>();
            await using var ms = new MemoryStream();
            Stream.CopyTo(ms);
            tsc.SetResult(ms.ToArray());
        }

        public override string ToString()
        {
            return $"#{Id} of type {Type.ToString()}.";
        }
    }
}
