using System.Net.Http.Headers;
using System.Text;
using RestSharp.Portable.Serializers;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Utils.Implementation.Services.Rest
{
    internal class RestSerializer : ISerializer
    {
        private readonly IJsonUtils jsonUtils;

        public RestSerializer(IJsonUtils jsonUtils)
        {
            this.jsonUtils = jsonUtils;
            ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = Encoding.UTF8.WebName,
            };
        }

        public byte[] Serialize(object payload)
        {
            return jsonUtils.Serialize(payload);
        }

        public MediaTypeHeaderValue ContentType { get; set; }
    }
}
