using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using ImageResizer;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Models.CompanyLogo;

namespace WB.UI.Headquarters.API.Resources
{
    [AllowAnonymous]
    public class CompanyLogoController : ApiController
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> logoStorage;
        private const int defaultImageSizeToScale = 156;

        public CompanyLogoController(IPlainKeyValueStorage<CompanyLogo> logoStorage)
        {
            this.logoStorage = logoStorage;
        }

        [HttpGet]
        public HttpResponseMessage Thumbnail()
        {
            var companyLogo = this.logoStorage.GetById(CompanyLogo.StorageKey)?.Logo;

            if (companyLogo != null)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(GetTrasformedContent(companyLogo, defaultImageSizeToScale))
                };

                response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");

                return response;
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
        }

        private static byte[] GetTrasformedContent(byte[] source, int? sizeToScale = null)
        {
            if (!sizeToScale.HasValue) return source;

            //later should handle video and produce image preview 
            using (var outputStream = new MemoryStream())
            {
                ImageBuilder.Current.Build(source, outputStream, new ResizeSettings
                {
                    MaxWidth = sizeToScale.Value,
                    MaxHeight = sizeToScale.Value,
                    Format = "png"
                });

                return outputStream.ToArray();
            }
        }
    }
}