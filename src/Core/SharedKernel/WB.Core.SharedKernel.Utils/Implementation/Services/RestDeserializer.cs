using RestSharp.Portable;
using RestSharp.Portable.Deserializers;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.SharedKernel.Utils.Implementation.Services
{
    internal class RestDeserializer : IDeserializer
    {
        private readonly IJsonUtils jsonUtils;

        public RestDeserializer(IJsonUtils jsonUtils)
        {
            this.jsonUtils = jsonUtils;
        }

        public T Deserialize<T>(IRestResponse response)
        {
            return this.jsonUtils.Deserrialize<T>(response.RawBytes);
        }
    }
}