using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace WB.UI.Headquarters.Utils
{
    public static class Extensions
    {
        public static T GetActionArgumentOrDefault<T>(this HttpActionExecutedContext context, string argument, T defaultValue)
        {
            object value;
            if (!context.ActionContext.ActionArguments.TryGetValue(argument, out value))
                return defaultValue;

            if (value is T)
            {
                return (T)value;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (InvalidCastException)
            {
                return defaultValue;
            }
        }

        public static string AbsoluteContent(this UrlHelper urlHelper, string contentPath)
        {
            var path = urlHelper.Content(contentPath);
            var url = new Uri(HttpContext.Current.Request.Url, path);
            return url.AbsoluteUri;
        }

        public static bool RequestHasMatchingFileHash(this HttpRequestMessage request, byte[] hash)
        {
            var expectedHash = $@"""{Convert.ToBase64String(hash)}""";

            if (request.Headers.IfNoneMatch.Any(tag => tag.Tag == expectedHash))
            {
                return true;
            }

            return false;
        }
    }
}
