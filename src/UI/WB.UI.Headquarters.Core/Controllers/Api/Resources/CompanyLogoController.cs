using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.Headquarters.Code.Workspaces;
using WB.UI.Headquarters.Models.CompanyLogo;
using WB.UI.Shared.Web.Services;

namespace WB.UI.Headquarters.Controllers.Api.Resources
{
    [Localizable(false)]
    [AllowPrimaryWorkspaceFallback]
    [Route("api/CompanyLogo")]
    public class CompanyLogoController : ControllerBase
    {
        private readonly IPlainKeyValueStorage<CompanyLogo> appSettingsStorage;
        private readonly IImageProcessingService imageProcessingService;
        private readonly IWebHostEnvironment webHost;
        private const int defaultImageHeightToScale = 329;
        private const int defaultImageWidthToScale = 365;

        public CompanyLogoController(IPlainKeyValueStorage<CompanyLogo> appSettingsStorage,
            IImageProcessingService imageProcessingService,
            IWebHostEnvironment webHost)
        {
            this.appSettingsStorage = appSettingsStorage;
            this.imageProcessingService = imageProcessingService;
            this.webHost = webHost;
        }

        [HttpGet]
        [Route("Thumbnail")]
        public IActionResult Thumbnail()
        {
            var companyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey);

            if (companyLogo != null)
            {
                var stringEtag = companyLogo.GetEtagValue();
                var etag = $"\"{stringEtag}\"";

                var incomingEtag = Request.Headers[@"If-None-Match"];
                if (string.Compare(incomingEtag, etag, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return StatusCode(StatusCodes.Status304NotModified);

                var resizeImage = imageProcessingService.ResizeImage(companyLogo.Logo, defaultImageHeightToScale, defaultImageWidthToScale);

                return File(resizeImage, "image/png", null,
                    new Microsoft.Net.Http.Headers.EntityTagHeaderValue(etag));
            }

            return NotFound();
        }

        [HttpGet]
        [Route("ThumbnailOrDefault")]
        public async Task<IActionResult> ThumbnailOrDefault()
        {
            var companyLogo = this.appSettingsStorage.GetById(CompanyLogo.CompanyLogoStorageKey);

            if (companyLogo == null)
            {
                using var readStream = webHost.WebRootFileProvider.GetFileInfo("img/logo.png").CreateReadStream();
                using var memoryStream = new MemoryStream();
                await readStream.CopyToAsync(memoryStream);
                companyLogo = new CompanyLogo
                {
                    Logo = memoryStream.ToArray()
                };
            }

            var stringEtag = '"' + companyLogo.GetEtagValue() + '"';

            var incomingEtag = Request.Headers[@"If-None-Match"];
            if (string.Compare(incomingEtag, stringEtag, StringComparison.InvariantCultureIgnoreCase) == 0)
                return StatusCode(StatusCodes.Status304NotModified);

            var content = imageProcessingService.ResizeImage(companyLogo.Logo, defaultImageHeightToScale,
                defaultImageWidthToScale);;
            return File(content, "image/png", null, new Microsoft.Net.Http.Headers.EntityTagHeaderValue(stringEtag));
        }
    }
}
