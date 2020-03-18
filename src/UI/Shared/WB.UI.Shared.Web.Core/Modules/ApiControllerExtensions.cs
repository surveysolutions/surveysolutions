using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;


namespace WB.UI.Shared.Web.Modules
{
    public static class ApiControllerExtensions
    {
        public static IActionResult BinaryResponseMessageWithEtag(this ControllerBase controller, byte[] resultFile, string contentType = "image/png")
        {
            var stringEtag = GetEtagValue(resultFile);
            var etag = $"\"{stringEtag}\"";

            return controller.File(resultFile, MediaTypeHeaderValue.Parse(contentType).ToString(), 
                null, new Microsoft.Net.Http.Headers.EntityTagHeaderValue(etag));
        }

        public static string GetEtagValue(this byte[] bytes)
        {
            using var hasher = SHA1.Create();
            var computeHash = hasher.ComputeHash(bytes);
            return BitConverter.ToString(computeHash).Replace("-", "");
        }
    }
}
