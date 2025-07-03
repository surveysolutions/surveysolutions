using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Infrastructure.Native.Storage
{
    public class NewtonJsonSerializer : ISerializer
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = 
            new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal,
            Formatting = Formatting.Indented,
            SerializationBinder = new OldToNewAssemblyRedirectSerializationBinder()
        };

        public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item, jsonSerializerSettings);
        }
        
        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, jsonSerializerSettings);
        }

        public string SerializeWithoutTypes(object item) => JsonConvert.SerializeObject(item, new JsonSerializerSettings());

        public T DeserializeWithoutTypes<T>(string payload) => JsonConvert.DeserializeObject<T>(payload, 
            new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.None
            });
    }
}
