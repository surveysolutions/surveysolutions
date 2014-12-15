using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.SharedKernel.Utils.Implementation.Services
{
    public class NewtonJsonUtils : IJsonUtils
    {
        static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                return new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    NullValueHandling = NullValueHandling.Ignore,
                    FloatParseHandling = FloatParseHandling.Decimal
                };
            }
        }
        public string GetItemAsContent(object item)
        {
            return JsonConvert.SerializeObject(item, Formatting.None, JsonSerializerSettings);
        }

        public T Deserrialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(payload, JsonSerializerSettings);
        }
    }
}
