using System;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.Infrastructure.HttpServices.HttpClient;

namespace WB.UI.Headquarters.Services.Impl
{
    public class QRCodeHelper : IQRCodeHelper
    {
        private readonly IOptions<HeadquartersConfig> options;

        public QRCodeHelper(IOptions<HeadquartersConfig> options)
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public string BaseUrl => options.Value.BaseUrl;

        public string GetBaseUrl() => BaseUrl;

        public string GetFullUrl(string relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(BaseUrl))
                return string.Empty;

            var fullUrl = new Url(BaseUrl, relativeUrl, null);

            return fullUrl.ToString();
        }

        public string GetQRCodeAsBase64StringSrc(string content, int height = 250, int width = 250, int margin = 0)
        {
            return $"data:image/png;base64,{QRCodeBuilder.GetQRCodeAsBase64String(content, height, width)}";
        }

        public bool SupportQRCodeGeneration()
        {
            return !string.IsNullOrWhiteSpace(BaseUrl);
        }
    }
}
