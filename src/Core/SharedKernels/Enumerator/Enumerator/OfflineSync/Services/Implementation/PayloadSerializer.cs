using System.Text;
using Newtonsoft.Json;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Services.Implementation
{
    public class PayloadSerializer : IPayloadSerializer
    {
        private readonly JsonSerializerSettings serializerSettings;

        public PayloadSerializer()
        {
            this.serializerSettings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
        }

        public T FromPayload<T>(byte[] payload)
        {
            var json = Encoding.UTF8.GetString(payload);
            return JsonConvert.DeserializeObject<T>(json, this.serializerSettings);
        }

        
        public byte[] ToPayload<T>(T message)
        {
            var json = JsonConvert.SerializeObject(message, serializerSettings);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
