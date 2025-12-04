using System.Collections.Generic;
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

        public string SerializeWithoutTypes(object item) => JsonConvert.SerializeObject(item, new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new Newtonsoft.Json.Converters.StringEnumConverter()
            }
        });

        public T DeserializeWithoutTypes<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload);
        }

        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, this.jsonSerializerSettings);
        }
    }
}
