﻿using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Web;
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
        private const int defaultImageHeightToScale = 329;
        private const int defaultImageWidthToScale = 365;

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
                var stringEtag = this.GetEtagValue(companyLogo);
                var etag = $"\"{stringEtag}\"";

                var incomingEtag = HttpContext.Current.Request.Headers[@"If-None-Match"];
                if (string.Compare(incomingEtag, etag, StringComparison.InvariantCultureIgnoreCase) == 0)
                    return new HttpResponseMessage(HttpStatusCode.NotModified);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(GetTrasformedContent(companyLogo, defaultImageHeightToScale, defaultImageWidthToScale))
                };

                response.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(@"image/png");
                response.Headers.ETag = new EntityTagHeaderValue(etag);

                return response;
            }

            return Request.CreateResponse(HttpStatusCode.NoContent);
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

        private static byte[] GetTrasformedContent(byte[] source, int? height = null, int? width = null)
        {
            if (!height.HasValue || !width.HasValue) return source;

            //later should handle video and produce image preview 
            using (var outputStream = new MemoryStream())
            {
                ImageBuilder.Current.Build(source, outputStream, new ResizeSettings
                {
                    MaxWidth = height.Value,
                    MaxHeight = width.Value,
                    Format = "png",
                    Mode = FitMode.Pad,
                    PaddingColor = Color.Transparent,
                    Anchor = ContentAlignment.MiddleCenter
                });

                return outputStream.ToArray();
            }
        }
    }
}