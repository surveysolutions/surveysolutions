using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    NullValueHandling = NullValueHandling.Ignore
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
