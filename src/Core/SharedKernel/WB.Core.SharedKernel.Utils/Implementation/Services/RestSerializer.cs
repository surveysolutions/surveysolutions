using RestSharp.Serializers;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.SharedKernel.Utils.Implementation.Services
{
    internal class RestSerializer : ISerializer
    {
        private readonly IJsonUtils jsonUtils;

        public RestSerializer(IJsonUtils jsonUtils)
        {
            this.jsonUtils = jsonUtils;
            this.ContentType = "application/json";
            this.RootElement = "request";
        }

        public string Serialize(object payload)
        {
            return this.jsonUtils.GetItemAsContent(payload);
        }

        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public string ContentType { get; set; }
    }
}
