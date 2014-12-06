using Newtonsoft.Json;

namespace WB.Core.SharedKernel.Utils.Serialization
{
    public  class NewtonJsonUtils : IJsonUtils
    {
        private JsonSerializerSettings JsonSerializerSettings
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
