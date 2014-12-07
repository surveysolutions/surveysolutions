using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Utils.Implementation.Services
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

        public byte[] Serialize(object payload)
        {
            var output = new System.IO.MemoryStream();
            using (var writer = new System.IO.StreamWriter(output))
                JsonConvert.SerializeObject(payload, Formatting.Indented, JsonSerializerSettings);
            return output.ToArray();
        }
    }
}
