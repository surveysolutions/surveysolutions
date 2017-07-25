using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Shared.Enumerator.Services.Internals
{
    public class PortableJsonSerializer : ISerializer
    {
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public PortableJsonSerializer()
        {
#pragma warning disable 612, 618
            this.jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Binder = new PortableOldToNewAssemblyRedirectSerializationBinder()
            };
#pragma warning restore 612, 618
    }

    public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item, jsonSerializerSettings);
        }
        
        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, this.jsonSerializerSettings);
        }
    }
}
