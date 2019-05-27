using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Controllers
{
    public static class RedirectHelper
    {
        public static void ApplyNonApiRedirect(this CookieApplyRedirectContext ctx)
        {
            if (IsAjaxRequest(ctx.Request) || IsApiRequest(ctx.Request) || IsBasicAuthApiUnAuthRequest(ctx.Response))
            {
                return;
            }

            var redirect = ctx.RedirectUri;

            bool isForwardedFromSecureProto()
            {
                if (ctx.Request.Headers.TryGetValue(@"X-Forwarded-Proto", out var values))
                {
                    return values.First().Equals(@"https", StringComparison.OrdinalIgnoreCase);
                }

                return false;
            }

            if (ctx.Request.IsSecure || isForwardedFromSecureProto())
            {
                if (redirect.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                {
                    redirect = httpsRedirectRx.Replace(redirect, "https://$1");
                }
            }

            ctx.Response.Redirect(redirect);
        }

        readonly static Regex httpsRedirectRx = new Regex("^http://(.*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static bool IsApiRequest(IOwinRequest request)
        {
            var userAgent = request.Headers[@"User-Agent"];
            return (userAgent?.ToLowerInvariant().Contains(@"org.worldbank.solutions.") ?? false) || (userAgent?.Contains(@"okhttp/") ?? false);
        }

        private static bool IsBasicAuthApiUnAuthRequest(IOwinResponse response)
        {
            return response.Headers[ApiBasicAuthAttribute.AuthHeader] != null;
        }

        private static bool IsAjaxRequest(IOwinRequest request)
        {
            IReadableStringCollection query = request.Query;
            if ((query != null) && (query["X-Requested-With"] == "XMLHttpRequest"))
            {
                return true;
            }
            IHeaderDictionary headers = request.Headers;
            return ((headers != null) && (headers["X-Requested-With"] == "XMLHttpRequest"));
        }
    }
}
