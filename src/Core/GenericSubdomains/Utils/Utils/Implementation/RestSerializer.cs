using System.Net.Http.Headers;
using System.Text;
using RestSharp.Portable.Serializers;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Utils.Implementation
{
    internal class RestSerializer : ISerializer
    {
        private readonly IJsonUtils jsonUtils;

        public RestSerializer(IJsonUtils jsonUtils)
        {
            this.jsonUtils = jsonUtils;
            this.ContentType = new MediaTypeHeaderValue("application/json") {CharSet = Encoding.UTF8.WebName};
        }

        public byte[] Serialize(object payload)
        {
            return this.jsonUtils.SerializeToByteArray(payload);
        }

        public MediaTypeHeaderValue ContentType { get; set; }
    }
}