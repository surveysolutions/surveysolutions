using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace WB.UI.Headquarters.Code
{
    public static class Extensions
    {
        public static bool RequestHasMatchingFileHash(this HttpRequest request, byte[] hash)
        {
            var expectedHash = $@"""{Convert.ToBase64String(hash)}""";

            if (request.Headers.ContainsKey(HeaderNames.IfNoneMatch))
            {
                var nonMatchHeader = request.Headers[HeaderNames.IfNoneMatch].ToString();
                var header = EntityTagHeaderValue.Parse(nonMatchHeader);
               
                return expectedHash.Equals(header.Tag.ToString(), StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public static Version GetProductVersionFromUserAgent(this HttpRequest request, string productName)
        {
            //foreach (var product in request.Headers[HeaderNames.UserAgent])
            //{
            //    if ((product.Product?.Name.StartsWith(productName, StringComparison.OrdinalIgnoreCase) ?? false)
            //        && Version.TryParse(product.Product.Version, out Version version))
            //    {
            //        return version;
            //    }
            //}

            return null;
        }
    }
}
