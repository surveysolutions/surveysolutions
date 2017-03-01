using System;
using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Web;
using System.Web.Http;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.API.Resources
{
    [Localizable(false)]
    [AllowAnonymous]
    public class CompanyLogoController : ApiController
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> logoStorage;
        private readonly IImageProcessingService imageProcessingService;
        private const int defaultImageHeightToScale = 329;
        private const int defaultImageWidthToScale = 365;

        public CompanyLogoController(IPlainKeyValueStorage<CompanyLogo> logoStorage, IImageProcessingService imageProcessingService)
        {
            this.logoStorage = logoStorage;
            this.imageProcessingService = imageProcessingService;
        }

        [HttpGet]
        public HttpResponseMessage Thumbnail()
        {
            var companyLogo = this.logoStorage.GetById(CompanyLogo.StorageKey)?.Logo;

            if (companyLogo != null)
            {
                var stringEtag = this.GetEtagValue(companyLogo);
                var etag = $"\"{stringEtag}\"";

                var incomingEtag = HttpContext.Current.Request.Headers[@"If-None-Match"];
                if (string.Compare(incomingEtag, etag, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return new HttpResponseMessage(HttpStatusCode.NotModified);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(imageProcessingService.ResizeImage(companyLogo, defaultImageHeightToScale, defaultImageWidthToScale))
                };

                response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(@"image/png");
                response.Headers.ETag = new EntityTagHeaderValue(etag);

                return response;
            }

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }

        private string GetEtagValue(byte[] bytes)
        {
            using (var hasher = SHA1.Create())
            {
                var computeHash = hasher.ComputeHash(bytes);
                string hash = BitConverter.ToString(computeHash).Replace("-", "");
                return hash;
            }
        }
    }
}