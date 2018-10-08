using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Infrastructure.Native.Storage
{
    public class NewtonJsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public NewtonJsonSerializer()
        {
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Formatting = Formatting.Indented,
                Binder = new OldToNewAssemblyRedirectSerializationBinder()
            };
        }

        public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item, jsonSerializerSettings);
        }
        
        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, jsonSerializerSettings);
        }
    }
}
